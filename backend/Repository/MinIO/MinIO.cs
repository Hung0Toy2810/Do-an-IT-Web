using Minio;
using Minio.DataModel.Args;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.IO;
using Minio.Exceptions;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Backend.Repository.MinIO
{
    public interface IFileRepository
    {
        Task<string> UploadFileAsync(IFormFile file, string bucketName); // Upload ảnh người dùng, trả về key
        Task<string> ConvertAndUploadPublicFileAsJpgAsync(Stream fileStream, string bucketName, string fileName, long maxSize); // Upload ảnh sản phẩm, trả về key
        Task<string> GetStaticPublicFileUrl(string bucketName, string objectName); // Lấy public URL từ key
        Task<string> GetPresignedUrlAsync(string bucketName, string fileName, TimeSpan expiry); // Lấy URL tạm thời
        Task DeleteFileAsync(string bucketName, string fileName); // Xóa ảnh
    }

    public class FileRepository : IFileRepository
    {
        private readonly IMinioClient _minioClient;
        private readonly string _minioPublicUrl;
        private readonly ILogger<FileRepository> _logger;
        private static readonly HashSet<string> AllowedImageContentTypes = new HashSet<string>
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/bmp",
            "image/webp"
        };

        public FileRepository(IMinioClient minioClient, IConfiguration configuration, ILogger<FileRepository> logger)
        {
            _minioClient = minioClient ?? throw new ArgumentNullException(nameof(minioClient));
            _minioPublicUrl = configuration["Minio:PublicUrl"] ?? throw new ArgumentNullException("Thiếu cấu hình Minio:PublicUrl.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Upload ảnh người dùng và trả về key
        public async Task<string> UploadFileAsync(IFormFile file, string bucketName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Tệp không được để trống.", nameof(file));

            if (!AllowedImageContentTypes.Contains(file.ContentType))
            {
                _logger.LogError("Invalid file type: {ContentType}. Only image files are allowed.", file.ContentType);
                throw new ArgumentException("Chỉ cho phép tệp hình ảnh (JPEG, PNG, GIF, BMP, WebP).", nameof(file));
            }

            await EnsureBucketExists(bucketName);
            await SetPublicBucketPolicy(bucketName);
            string fileName = $"{Guid.NewGuid()}_{file.FileName}";

            using var stream = file.OpenReadStream();
            try
            {
                using var image = await Image.LoadAsync(stream);
                stream.Position = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File {FileName} is not a valid image.", file.FileName);
                throw new ArgumentException("Tệp không phải là hình ảnh hợp lệ.", nameof(file));
            }

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(file.Length)
                .WithContentType(file.ContentType));

            _logger.LogInformation("Uploaded user image: {FileName}", fileName);
            return fileName;
        }

        // Upload ảnh sản phẩm (chuyển sang JPG, công khai)
        public async Task<string> ConvertAndUploadPublicFileAsJpgAsync(Stream fileStream, string bucketName, string fileName, long maxSize)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("Luồng tệp không được để trống.", nameof(fileStream));

            try
            {
                await EnsureBucketExists(bucketName);
                await SetPublicBucketPolicy(bucketName);
                string jpgFileName = Path.ChangeExtension(fileName, ".jpg");

                try
                {
                    using var image = await Image.LoadAsync(fileStream);
                    fileStream.Position = 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "File {FileName} is not a valid image.", fileName);
                    throw new ArgumentException("Tệp không phải là hình ảnh hợp lệ.", nameof(fileName));
                }

                if (fileStream.Length <= maxSize)
                {
                    using var outputStream = new MemoryStream();
                    using (var image = await Image.LoadAsync(fileStream))
                    {
                        image.SaveAsJpeg(outputStream, new JpegEncoder { Quality = 100 });
                    }
                    outputStream.Position = 0;

                    await _minioClient.PutObjectAsync(new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(jpgFileName)
                        .WithStreamData(outputStream)
                        .WithObjectSize(outputStream.Length)
                        .WithContentType("image/jpeg"));
                    _logger.LogInformation("Public image uploaded without compression: {FileName}", jpgFileName);
                    return jpgFileName;
                }

                using var tempStream = new MemoryStream();
                using (var image = await Image.LoadAsync(fileStream))
                {
                    int quality = 90;
                    long targetSize = maxSize;
                    long currentSize;

                    do
                    {
                        tempStream.SetLength(0);
                        image.SaveAsJpeg(tempStream, new JpegEncoder { Quality = quality });
                        currentSize = tempStream.Length;

                        if (currentSize > targetSize && quality > 10)
                            quality -= 10;
                        else if (currentSize < targetSize * 0.9 && quality < 90)
                            quality += 5;
                        else
                            break;

                    } while (quality > 0);

                    tempStream.Position = 0;
                    await _minioClient.PutObjectAsync(new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(jpgFileName)
                        .WithStreamData(tempStream)
                        .WithObjectSize(tempStream.Length)
                        .WithContentType("image/jpeg"));
                    _logger.LogInformation("Public image compressed and uploaded: {FileName}, Quality: {Quality}", jpgFileName, quality);
                }

                return jpgFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading public image to JPG: {FileName}", fileName);
                throw new InvalidOperationException($"Lỗi khi tải ảnh công khai '{fileName}': {ex.Message}", ex);
            }
        }

        // Lấy public URL từ key
        public async Task<string> GetStaticPublicFileUrl(string bucketName, string objectName)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
                throw new ArgumentException("Tên bucket không được để trống.", nameof(bucketName));
            if (string.IsNullOrWhiteSpace(objectName))
                throw new ArgumentException("Tên đối tượng không được để trống.", nameof(objectName));

            await EnsureBucketExists(bucketName);
            await SetPublicBucketPolicy(bucketName);

            var publicUrl = $"{_minioPublicUrl}/{bucketName}/{objectName}";
            _logger.LogInformation("Generated static public URL for {ObjectName}: {PublicUrl}", objectName, publicUrl);
            return publicUrl;
        }

        // Lấy URL tạm thời
        public async Task<string> GetPresignedUrlAsync(string bucketName, string fileName, TimeSpan expiry)
        {
            var presignedUrlArgs = new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithExpiry((int)expiry.TotalSeconds);
            var url = await _minioClient.PresignedGetObjectAsync(presignedUrlArgs);
            _logger.LogInformation("Generated presigned URL for {FileName}: {Url}", fileName, url);
            return url;
        }

        // Xóa ảnh
        public async Task DeleteFileAsync(string bucketName, string fileName)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
                throw new ArgumentException("Tên bucket không được để trống.", nameof(bucketName));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Tên tệp không được để trống.", nameof(fileName));

            try
            {
                var removeObjectArgs = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName);

                await _minioClient.RemoveObjectAsync(removeObjectArgs);
                _logger.LogInformation("Deleted file: {FileName} from bucket: {BucketName}", fileName, bucketName);
            }
            catch (MinioException ex)
            {
                _logger.LogError(ex, "Failed to delete object '{FileName}' from bucket '{BucketName}'", fileName, bucketName);
                throw new InvalidOperationException($"Không thể xóa tệp '{fileName}' trong bucket '{bucketName}': {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting object '{FileName}' from bucket '{BucketName}'", fileName, bucketName);
                throw new InvalidOperationException($"Đã xảy ra lỗi không mong muốn khi xóa tệp '{fileName}' trong bucket '{bucketName}': {ex.Message}", ex);
            }
        }

        // Đảm bảo bucket tồn tại
        private async Task EnsureBucketExists(string bucketName)
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
            bool found = await _minioClient.BucketExistsAsync(bucketExistsArgs);
            if (!found)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs);
                _logger.LogInformation("Created bucket: {BucketName}", bucketName);
            }
        }

        // Thiết lập chính sách công khai cho bucket
        private async Task SetPublicBucketPolicy(string bucketName)
        {
            try
            {
                var policyJson = $@"{{
                    ""Version"": ""2012-10-17"",
                    ""Statement"": [
                        {{
                            ""Effect"": ""Allow"",
                            ""Principal"": {{""AWS"": ""*""}},
                            ""Action"": [""s3:GetObject""],
                            ""Resource"": [""arn:aws:s3:::{bucketName}/*""]
                        }}
                    ]
                }}";

                await _minioClient.SetPolicyAsync(new SetPolicyArgs()
                    .WithBucket(bucketName)
                    .WithPolicy(policyJson));
                _logger.LogInformation("Set public read policy for bucket: {BucketName}", bucketName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set public policy for bucket {BucketName}", bucketName);
                throw new InvalidOperationException($"Không thể thiết lập chính sách công khai cho bucket '{bucketName}': {ex.Message}", ex);
            }
        }
    }
}
