using Backend.Repository.MinIO;

namespace Backend.Service.MinIO
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string bucketName); // Upload ảnh người dùng, trả về key
        Task<string> ConvertAndUploadPublicFileAsJpgAsync(Stream fileStream, string bucketName, string fileName, long maxSize); // Upload ảnh sản phẩm, trả về key
        Task<string> GetStaticPublicFileUrl(string bucketName, string objectName); // Lấy public URL từ key
        Task<string> GetPresignedUrlAsync(string bucketName, string fileName, TimeSpan expiry); // Lấy URL tạm thời
        Task DeleteFileAsync(string bucketName, string fileName); // Xóa ảnh
    }

    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepository;

        public FileService(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        }

        public Task<string> UploadFileAsync(IFormFile file, string bucketName)
        {
            return _fileRepository.UploadFileAsync(file, bucketName);
        }
        public Task<string> ConvertAndUploadPublicFileAsJpgAsync(Stream fileStream, string bucketName, string fileName, long maxSize)
        {
            return _fileRepository.ConvertAndUploadPublicFileAsJpgAsync(fileStream, bucketName, fileName, maxSize);
        }
        public Task<string> GetStaticPublicFileUrl(string bucketName, string objectName)
        {
            return _fileRepository.GetStaticPublicFileUrl(bucketName, objectName);
        }
        public Task<string> GetPresignedUrlAsync(string bucketName, string fileName, TimeSpan expiry)
        {
            return _fileRepository.GetPresignedUrlAsync(bucketName, fileName, expiry);
        }
        public Task DeleteFileAsync(string bucketName, string fileName)
        {
            return _fileRepository.DeleteFileAsync(bucketName, fileName);
        }
    }
}