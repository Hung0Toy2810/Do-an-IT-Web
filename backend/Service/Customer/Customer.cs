using Backend.Repository.CustomerRepository;
using Backend.Model.dto.Customer;
using Backend.Service.Password;
using Backend.Model.Entity;
using Backend.Service.Token;
using Backend.Repository.MinIO;
using Backend.Model.dto;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;


namespace Backend.Service.CustomerService
{
    public interface ICustomerService
    {
        Task CreateCustomerAsync(CreateCustomer createCustomer);
        Task<LoginResponse> LoginAsync(LoginCustomer loginCustomer, string clientIp);
        Task<string> UpdateAvatarAsync(Guid customerId, IFormFile file);
        Task UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest request);
        Task DeleteCustomerAsync(Guid customerId);
        Task ChangePasswordAsync(Guid customerId, ChangePasswordRequest request);
        Task<CustomerInfoDto> GetCustomerInfoAsync(Guid customerId);
        Task<CustomerInfoDto> GetCustomerInfoByTokenAsync(string userIdClaim);
        Task<List<CustomerInfoDto>> GetAllCustomersAsync();
    }
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly SQLServerDbContext _context;
        private readonly IFileRepository _fileRepository;
        private readonly IConfiguration _configuration;

        public CustomerService(
            ICustomerRepository customerRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            SQLServerDbContext context,
            IFileRepository fileRepository,
            IConfiguration configuration)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task CreateCustomerAsync(CreateCustomer createCustomer)
        {
            // Kiểm tra dữ liệu đầu vào
            if (createCustomer == null)
                throw new ArgumentNullException(nameof(createCustomer), "Customer data cannot be null.");

            if (string.IsNullOrWhiteSpace(createCustomer.PhoneNumber))
                throw new ArgumentException("Phone number cannot be empty.", nameof(createCustomer.PhoneNumber));

            if (string.IsNullOrWhiteSpace(createCustomer.Password))
                throw new ArgumentException("Password cannot be empty.", nameof(createCustomer.Password));

            // Kiểm tra xem số điện thoại đã được sử dụng bởi tài khoản active chưa
            if (await _customerRepository.IsPhoneNumberTakenAsync(createCustomer.PhoneNumber))
                throw new InvalidOperationException("A customer with this phone number already exists.");

            // Tạo đối tượng Customer với các trường tối thiểu
            var customer = new Customer
            {
                PhoneNumber = createCustomer.PhoneNumber,
                HashPassword = _passwordHasher.HashPassword(createCustomer.Password),
                Status = true,
                CustomerName = string.Empty,
                DeliveryAddress = string.Empty,
                Email = string.Empty,
                AvtURL = string.Empty
            };

            // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _customerRepository.CreateCustomerAsync(customer);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginCustomer loginCustomer, string clientIp)
        {
            // Kiểm tra dữ liệu đầu vào
            if (loginCustomer == null)
                throw new ArgumentNullException(nameof(loginCustomer), "Login data cannot be null.");

            if (string.IsNullOrWhiteSpace(loginCustomer.PhoneNumber))
                throw new ArgumentException("Phone number cannot be empty.", nameof(loginCustomer.PhoneNumber));

            if (string.IsNullOrWhiteSpace(loginCustomer.Password))
                throw new ArgumentException("Password cannot be empty.", nameof(loginCustomer.Password));

            // Tìm khách hàng theo số điện thoại (chỉ lấy tài khoản active)
            var customer = await _customerRepository.GetCustomerByPhoneNumberAsync(loginCustomer.PhoneNumber);
            if (customer == null)
                throw new UnauthorizedAccessException("Invalid phone number or password.");

            // Xác minh mật khẩu
            if (!_passwordHasher.VerifyPassword(loginCustomer.Password, customer.HashPassword))
                throw new UnauthorizedAccessException("Invalid phone number or password.");

            // Kiểm tra trạng thái khách hàng
            if (!customer.Status)
                throw new UnauthorizedAccessException("Customer account is inactive.");

            // Tạo JWT token
            var token = await _jwtTokenService.GenerateTokenAsync(
                id: customer.Id.ToString(),
                username: customer.PhoneNumber,
                role: "Customer",
                clientIp: clientIp
            );

            // Giải mã token để lấy thời gian hết hạn
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var expiration = jwtToken.ValidTo;

            return new LoginResponse
            {
                Token = token,
                Expiration = expiration,
                Message = "Login successful."
            };
        }

        public async Task<string> UpdateAvatarAsync(Guid customerId, IFormFile file)
        {
            var bucketName = "avatars"; // Bucket dành cho ảnh đại diện
            var publicUrlBase = _configuration["Minio:PublicUrl"] ?? throw new InvalidOperationException("Minio public URL is not configured.");

            // Kiểm tra khách hàng tồn tại
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found.");

            string oldAvtURL = customer.AvtURL; // Lưu URL cũ (key cũ)
            string newAvtURL = string.Empty; // Key mới sẽ được gán sau khi upload thành công

            // Bắt đầu transaction cho DB
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Upload ảnh mới
                var fileKey = await _fileRepository.UploadFileAsync(file, bucketName);

                // Tạo URL công khai cho ảnh mới
                newAvtURL = $"{publicUrlBase}/{bucketName}/{fileKey}";

                // Cập nhật URL mới vào customer
                customer.AvtURL = newAvtURL;
                await _customerRepository.UpdateCustomerAsync(customer);

                // Nếu cập nhật DB thành công và có ảnh cũ, xóa ảnh cũ
                if (!string.IsNullOrEmpty(oldAvtURL))
                {
                    // Trích xuất fileKey từ URL cũ (bỏ base URL và bucket name)
                    var oldFileKey = oldAvtURL.Replace($"{publicUrlBase}/{bucketName}/", "");
                    await _fileRepository.DeleteFileAsync(bucketName, oldFileKey);
                }

                // Commit transaction
                await transaction.CommitAsync();

                return newAvtURL;
            }
            catch
            {
                // Rollback transaction DB
                await transaction.RollbackAsync();

                // Rollback file: Nếu upload thành công nhưng DB thất bại
                if (!string.IsNullOrEmpty(newAvtURL))
                {
                    var newFileKey = newAvtURL.Replace($"{publicUrlBase}/{bucketName}/", "");
                    await _fileRepository.DeleteFileAsync(bucketName, newFileKey); // Xóa ảnh mới
                }

                throw; // Ném lại ngoại lệ để middleware xử lý
            }
        }

        public async Task UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest request)
        {
            // Kiểm tra dữ liệu đầu vào
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Update request cannot be null.");

            // Kiểm tra khách hàng tồn tại
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found.");

            // Kiểm tra trạng thái khách hàng
            if (!customer.Status)
                throw new UnauthorizedAccessException("Customer account is inactive.");

            // Kiểm tra email nếu được cung cấp
            if (!string.IsNullOrEmpty(request.Email) && request.Email != customer.Email)
            {
                if (await _customerRepository.IsEmailTakenAsync(request.Email))
                    throw new InvalidOperationException("The email address is already in use by an active account.");
            }

            // Bắt đầu transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Cập nhật các thuộc tính (chỉ cập nhật nếu giá trị được cung cấp)
                if (!string.IsNullOrEmpty(request.CustomerName))
                    customer.CustomerName = request.CustomerName;

                if (!string.IsNullOrEmpty(request.DeliveryAddress))
                    customer.DeliveryAddress = request.DeliveryAddress;

                if (!string.IsNullOrEmpty(request.Email))
                    customer.Email = request.Email;

                await _customerRepository.UpdateCustomerAsync(customer);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteCustomerAsync(Guid customerId)
        {
            // Kiểm tra khách hàng tồn tại
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found.");

            // Kiểm tra trạng thái khách hàng
            if (!customer.Status)
                throw new InvalidOperationException("Customer account is already inactive.");

            // Bắt đầu transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Đặt Status = false
                customer.Status = false;
                await _customerRepository.UpdateCustomerAsync(customer);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ChangePasswordAsync(Guid customerId, ChangePasswordRequest request)
        {
            // Kiểm tra dữ liệu đầu vào
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Password change request cannot be null.");

            if (string.IsNullOrWhiteSpace(request.OldPassword))
                throw new ArgumentException("Old password cannot be empty.", nameof(request.OldPassword));

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new ArgumentException("New password cannot be empty.", nameof(request.NewPassword));

            // Kiểm tra khách hàng tồn tại
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found.");

            // Kiểm tra trạng thái khách hàng
            if (!customer.Status)
                throw new UnauthorizedAccessException("Customer account is inactive.");

            // Xác minh mật khẩu cũ
            if (!_passwordHasher.VerifyPassword(request.OldPassword, customer.HashPassword))
                throw new UnauthorizedAccessException("Incorrect old password.");

            // Bắt đầu transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Cập nhật mật khẩu mới
                customer.HashPassword = _passwordHasher.HashPassword(request.NewPassword);
                await _customerRepository.UpdateCustomerAsync(customer);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CustomerInfoDto> GetCustomerInfoAsync(Guid customerId)
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found.");

            if (!customer.Status)
                throw new UnauthorizedAccessException("Customer account is inactive.");

            return new CustomerInfoDto
            {
                CustomerName = customer.CustomerName,
                DeliveryAddress = customer.DeliveryAddress,
                PhoneNumber = customer.PhoneNumber,
                AvtURL = customer.AvtURL,
                Email = customer.Email
            };
        }

        public async Task<CustomerInfoDto> GetCustomerInfoByTokenAsync(string userIdClaim)
        {
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid customerId))
                throw new UnauthorizedAccessException("Invalid user ID in token.");

            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found.");

            if (!customer.Status)
                throw new UnauthorizedAccessException("Customer account is inactive.");

            return new CustomerInfoDto
            {
                CustomerName = customer.CustomerName,
                DeliveryAddress = customer.DeliveryAddress,
                PhoneNumber = customer.PhoneNumber,
                AvtURL = customer.AvtURL,
                Email = customer.Email
            };
        }

        public async Task<List<CustomerInfoDto>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.GetAllCustomersAsync();
            var customerDtos = new List<CustomerInfoDto>();

            foreach (var customer in customers)
            {
                customerDtos.Add(new CustomerInfoDto
                {
                    CustomerName = customer.CustomerName,
                    DeliveryAddress = customer.DeliveryAddress,
                    PhoneNumber = customer.PhoneNumber,
                    AvtURL = customer.AvtURL,
                    Email = customer.Email
                });
            }

            return customerDtos;
        }
    }
}