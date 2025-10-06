using Minio;
using Minio.DataModel.Args;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.IO;
using Minio.Exceptions;

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

        public FileRepository(IMinioClient minioClient, IConfiguration configuration, ILogger<FileRepository> logger)
        {
            _minioClient = minioClient ?? throw new ArgumentNullException(nameof(minioClient));
            _minioPublicUrl = configuration["Minio:PublicUrl"] ?? throw new ArgumentNullException("Minio:PublicUrl is not configured");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Upload ảnh người dùng và trả về key
        public async Task<string> UploadFileAsync(IFormFile file, string bucketName)
        {
            await EnsureBucketExists(bucketName);
            string fileName = $"{Guid.NewGuid()}_{file.FileName}";
            using var stream = file.OpenReadStream();
            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(file.Length)
                .WithContentType(file.ContentType));
            _logger.LogInformation("Uploaded user file: {FileName}", fileName);
            return fileName;
        }

        // Upload ảnh sản phẩm (chuyển đổi sang JPG, công khai) và trả về key
        public async Task<string> ConvertAndUploadPublicFileAsJpgAsync(Stream fileStream, string bucketName, string fileName, long maxSize)
        {
            try
            {
                await EnsureBucketExists(bucketName);
                await SetPublicBucketPolicy(bucketName);
                string jpgFileName = Path.ChangeExtension(fileName, ".jpg");

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
                    _logger.LogInformation("Public file uploaded without compression: {FileName}", jpgFileName);
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
                        {
                            quality -= 10;
                        }
                        else if (currentSize < targetSize * 0.9 && quality < 90)
                        {
                            quality += 5;
                        }
                        else
                        {
                            break;
                        }
                    } while (quality > 0);

                    tempStream.Position = 0;
                    await _minioClient.PutObjectAsync(new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(jpgFileName)
                        .WithStreamData(tempStream)
                        .WithObjectSize(tempStream.Length)
                        .WithContentType("image/jpeg"));
                    _logger.LogInformation("Public file compressed to ~{MaxSize}MB and uploaded: {FileName}, Quality: {Quality}",
                        maxSize / (1024.0 * 1024.0), jpgFileName, quality);
                }

                return jpgFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading public file to JPG");
                throw;
            }
        }

        // Lấy public URL từ key (dành cho ảnh sản phẩm)
        public async Task<string> GetStaticPublicFileUrl(string bucketName, string objectName)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
                throw new ArgumentException("Bucket name cannot be empty", nameof(bucketName));
            if (string.IsNullOrWhiteSpace(objectName))
                throw new ArgumentException("Object name cannot be empty", nameof(objectName));

            await EnsureBucketExists(bucketName);
            await SetPublicBucketPolicy(bucketName);

            var publicUrl = $"{_minioPublicUrl}/{bucketName}/{objectName}";
            _logger.LogInformation("Generated static public URL for {ObjectName}: {PublicUrl}", objectName, publicUrl);
            return publicUrl;
        }

        // Lấy URL tạm thời từ key
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
                throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

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
                throw new InvalidOperationException($"Failed to delete object '{fileName}' from bucket '{bucketName}': {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting object '{FileName}' from bucket '{BucketName}'", fileName, bucketName);
                throw new InvalidOperationException($"An unexpected error occurred while deleting object '{fileName}' from bucket '{bucketName}': {ex.Message}", ex);
            }
        }

        // Hàm hỗ trợ: Đảm bảo bucket tồn tại
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

        // Hàm hỗ trợ: Thiết lập chính sách công khai cho bucket
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
                throw;
            }
        }
    }
}