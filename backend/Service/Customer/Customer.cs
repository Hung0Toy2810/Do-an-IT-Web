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
using StackExchange.Redis;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace Backend.Service.CustomerService
{
    public interface ICustomerService
    {
        Task GenerateOtpForRegistrationAsync(string phoneNumber);
        Task CreateCustomerAsync(CreateCustomer createCustomer, string otp);
        Task<LoginResponse> LoginAsync(LoginCustomer loginCustomer, string clientIp);
        Task GenerateOtpForPasswordRecoveryAsync(string phoneNumber);
        Task ResetPasswordAsync(string phoneNumber, string otp, string newPassword);
        Task<string> UpdateAvatarAsync(Guid customerId, IFormFile file);
        Task UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest request);
        Task DeleteCustomerAsync(Guid customerId);
        Task ChangePasswordAsync(Guid customerId, ChangePasswordRequest request);
        Task<CustomerInfoDto> GetCustomerInfoAsync(Guid customerId);
        Task<CustomerInfoDto> GetCustomerInfoByTokenAsync(string userIdClaim);
        Task<List<CustomerInfoDto>> GetAllCustomersAsync();
        Task LogoutCurrentDeviceAsync(string token);
        Task LogoutAllOtherDevicesAsync(string userId, string currentTokenJti);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly SQLServerDbContext _context;
        private readonly IFileRepository _fileRepository;
        private readonly IConfiguration _configuration;
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(
            ICustomerRepository customerRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            SQLServerDbContext context,
            IFileRepository fileRepository,
            IConfiguration configuration,
            IConnectionMultiplexer redis,
            ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task GenerateOtpForRegistrationAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Số điện thoại không được để trống.", nameof(phoneNumber));

            if (await _customerRepository.IsPhoneNumberTakenAsync(phoneNumber))
                throw new InvalidOperationException("Số điện thoại đã được sử dụng bởi một tài khoản đang hoạt động.");

            var otp = GenerateOtp();
            var db = _redis.GetDatabase();
            await db.StringSetAsync($"otp:register:{phoneNumber}", otp, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Đã sinh OTP cho đăng ký: {Otp} cho số điện thoại {PhoneNumber}", otp, phoneNumber);
        }

        public async Task CreateCustomerAsync(CreateCustomer createCustomer, string otp)
        {
            if (createCustomer == null)
                throw new ArgumentNullException(nameof(createCustomer), "Dữ liệu khách hàng không được để trống.");

            if (string.IsNullOrWhiteSpace(createCustomer.PhoneNumber))
                throw new ArgumentException("Số điện thoại không được để trống.", nameof(createCustomer.PhoneNumber));

            if (string.IsNullOrWhiteSpace(createCustomer.Password))
                throw new ArgumentException("Mật khẩu không được để trống.", nameof(createCustomer.Password));

            if (string.IsNullOrWhiteSpace(otp))
                throw new ArgumentException("OTP không được để trống.", nameof(otp));

            var db = _redis.GetDatabase();
            var storedOtp = await db.StringGetAsync($"otp:register:{createCustomer.PhoneNumber}");
            if (storedOtp.IsNullOrEmpty || storedOtp != otp)
                throw new UnauthorizedAccessException("OTP không hợp lệ.");

            if (await _customerRepository.IsPhoneNumberTakenAsync(createCustomer.PhoneNumber))
                throw new InvalidOperationException("Số điện thoại đã được sử dụng bởi một tài khoản đang hoạt động.");

            var customer = new Customer
            {
                PhoneNumber = createCustomer.PhoneNumber,
                HashPassword = _passwordHasher.HashPassword(createCustomer.Password),
                Status = true,
                CustomerName = string.Empty,
                StandardShippingAddress = new ShippingAddress(),
                Email = string.Empty,
                AvtURL = string.Empty
            };

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _customerRepository.CreateCustomerAsync(customer);
                    await db.KeyDeleteAsync($"otp:register:{createCustomer.PhoneNumber}");
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
            if (loginCustomer == null)
                throw new ArgumentNullException(nameof(loginCustomer), "Dữ liệu đăng nhập không được để trống.");

            if (string.IsNullOrWhiteSpace(loginCustomer.PhoneNumber))
                throw new ArgumentException("Số điện thoại không được để trống.", nameof(loginCustomer.PhoneNumber));

            if (string.IsNullOrWhiteSpace(loginCustomer.Password))
                throw new ArgumentException("Mật khẩu không được để trống.", nameof(loginCustomer.Password));

            var customer = await _customerRepository.GetCustomerByPhoneNumberAsync(loginCustomer.PhoneNumber);
            if (customer == null)
                throw new UnauthorizedAccessException("Số điện thoại hoặc mật khẩu không đúng.");

            if (!_passwordHasher.VerifyPassword(loginCustomer.Password, customer.HashPassword))
                throw new UnauthorizedAccessException("Số điện thoại hoặc mật khẩu không đúng.");

            if (!customer.Status)
                throw new UnauthorizedAccessException("Tài khoản khách hàng không hoạt động.");

            var token = await _jwtTokenService.GenerateTokenAsync(
                id: customer.Id.ToString(),
                username: customer.PhoneNumber,
                role: "Customer",
                clientIp: clientIp
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var expiration = jwtToken.ValidTo;

            return new LoginResponse
            {
                Token = token,
                Expiration = expiration,
                Message = "Đăng nhập thành công."
            };
        }

        public async Task GenerateOtpForPasswordRecoveryAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Số điện thoại không được để trống.", nameof(phoneNumber));

            var customer = await _customerRepository.GetCustomerByPhoneNumberAsync(phoneNumber);
            if (customer == null || !customer.Status)
                throw new UnauthorizedAccessException("Số điện thoại không hợp lệ hoặc tài khoản không hoạt động.");

            var otp = GenerateOtp();
            var db = _redis.GetDatabase();
            await db.StringSetAsync($"otp:recovery:{phoneNumber}", otp, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Đã sinh OTP cho khôi phục mật khẩu: {Otp} cho số điện thoại {PhoneNumber}", otp, phoneNumber);
        }

        public async Task ResetPasswordAsync(string phoneNumber, string otp, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Số điện thoại không được để trống.", nameof(phoneNumber));

            if (string.IsNullOrWhiteSpace(otp))
                throw new ArgumentException("OTP không được để trống.", nameof(otp));

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Mật khẩu mới không được để trống.", nameof(newPassword));

            var db = _redis.GetDatabase();
            var storedOtp = await db.StringGetAsync($"otp:recovery:{phoneNumber}");
            if (storedOtp.IsNullOrEmpty || storedOtp != otp)
                throw new UnauthorizedAccessException("OTP không hợp lệ.");

            var customer = await _customerRepository.GetCustomerByPhoneNumberAsync(phoneNumber);
            if (customer == null || !customer.Status)
                throw new UnauthorizedAccessException("Tài khoản khách hàng không hoạt động.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                customer.HashPassword = _passwordHasher.HashPassword(newPassword);
                await _customerRepository.UpdateCustomerAsync(customer);
                await db.KeyDeleteAsync($"otp:recovery:{phoneNumber}");
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<string> UpdateAvatarAsync(Guid customerId, IFormFile file)
        {
            var bucketName = "avatars";
            var publicUrlBase = _configuration["Minio:PublicUrl"] ?? throw new InvalidOperationException("Minio public URL không được cấu hình.");

            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Không tìm thấy khách hàng.");

            string oldAvtURL = customer.AvtURL;
            string newAvtURL = string.Empty;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var fileKey = await _fileRepository.UploadFileAsync(file, bucketName);
                newAvtURL = $"{publicUrlBase}/{bucketName}/{fileKey}";
                customer.AvtURL = newAvtURL;
                await _customerRepository.UpdateCustomerAsync(customer);

                if (!string.IsNullOrEmpty(oldAvtURL))
                {
                    var oldFileKey = oldAvtURL.Replace($"{publicUrlBase}/{bucketName}/", "");
                    await _fileRepository.DeleteFileAsync(bucketName, oldFileKey);
                }

                await transaction.CommitAsync();
                return newAvtURL;
            }
            catch
            {
                await transaction.RollbackAsync();
                if (!string.IsNullOrEmpty(newAvtURL))
                {
                    var newFileKey = newAvtURL.Replace($"{publicUrlBase}/{bucketName}/", "");
                    await _fileRepository.DeleteFileAsync(bucketName, newFileKey);
                }
                throw;
            }
        }

        public async Task UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Yêu cầu cập nhật không được để trống.");

            if (request.StandardShippingAddress == null)
                throw new ArgumentException("Địa chỉ giao hàng không được để trống.", nameof(request.StandardShippingAddress));

            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Không tìm thấy khách hàng.");

            if (!customer.Status)
                throw new UnauthorizedAccessException("Tài khoản khách hàng không hoạt động.");

            if (!string.IsNullOrEmpty(request.Email) && request.Email != customer.Email)
            {
                if (await _customerRepository.IsEmailTakenAsync(request.Email))
                    throw new InvalidOperationException("Địa chỉ email đã được sử dụng bởi một tài khoản đang hoạt động.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(request.CustomerName))
                    customer.CustomerName = request.CustomerName;

                customer.StandardShippingAddress = new ShippingAddress
                {
                    ProvinceId = request.StandardShippingAddress.ProvinceId,
                    ProvinceCode = request.StandardShippingAddress.ProvinceCode,
                    ProvinceName = request.StandardShippingAddress.ProvinceName,
                    DistrictId = request.StandardShippingAddress.DistrictId,
                    DistrictValue = request.StandardShippingAddress.DistrictValue,
                    DistrictName = request.StandardShippingAddress.DistrictName,
                    WardsId = request.StandardShippingAddress.WardsId,
                    WardsName = request.StandardShippingAddress.WardsName,
                    DetailAddress = request.StandardShippingAddress.DetailAddress
                };

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
            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Không tìm thấy khách hàng.");

            if (!customer.Status)
                throw new InvalidOperationException("Tài khoản khách hàng đã không hoạt động.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
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
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Yêu cầu thay đổi mật khẩu không được để trống.");

            if (string.IsNullOrWhiteSpace(request.OldPassword))
                throw new ArgumentException("Mật khẩu cũ không được để trống.", nameof(request.OldPassword));

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new ArgumentException("Mật khẩu mới không được để trống.", nameof(request.NewPassword));

            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Không tìm thấy khách hàng.");

            if (!customer.Status)
                throw new UnauthorizedAccessException("Tài khoản khách hàng không hoạt động.");

            if (!_passwordHasher.VerifyPassword(request.OldPassword, customer.HashPassword))
                throw new UnauthorizedAccessException("Mật khẩu cũ không đúng.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
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
                throw new ArgumentException("Không tìm thấy khách hàng.");

            if (!customer.Status)
                throw new UnauthorizedAccessException("Tài khoản khách hàng không hoạt động.");

            return new CustomerInfoDto
            {
                CustomerName = customer.CustomerName,
                StandardShippingAddress = new ShippingAddressDto
                {
                    ProvinceId = customer.StandardShippingAddress.ProvinceId,
                    ProvinceCode = customer.StandardShippingAddress.ProvinceCode,
                    ProvinceName = customer.StandardShippingAddress.ProvinceName,
                    DistrictId = customer.StandardShippingAddress.DistrictId,
                    DistrictValue = customer.StandardShippingAddress.DistrictValue,
                    DistrictName = customer.StandardShippingAddress.DistrictName,
                    WardsId = customer.StandardShippingAddress.WardsId,
                    WardsName = customer.StandardShippingAddress.WardsName,
                    DetailAddress = customer.StandardShippingAddress.DetailAddress
                },
                PhoneNumber = customer.PhoneNumber,
                AvtURL = customer.AvtURL,
                Email = customer.Email
            };
        }

        public async Task<CustomerInfoDto> GetCustomerInfoByTokenAsync(string userIdClaim)
        {
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid customerId))
                throw new UnauthorizedAccessException("ID người dùng trong token không hợp lệ.");

            var customer = await _customerRepository.GetCustomerByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Không tìm thấy khách hàng.");

            if (!customer.Status)
                throw new UnauthorizedAccessException("Tài khoản khách hàng không hoạt động.");

            return new CustomerInfoDto
            {
                CustomerName = customer.CustomerName,
                StandardShippingAddress = new ShippingAddressDto
                {
                    ProvinceId = customer.StandardShippingAddress.ProvinceId,
                    ProvinceCode = customer.StandardShippingAddress.ProvinceCode,
                    ProvinceName = customer.StandardShippingAddress.ProvinceName,
                    DistrictId = customer.StandardShippingAddress.DistrictId,
                    DistrictValue = customer.StandardShippingAddress.DistrictValue,
                    DistrictName = customer.StandardShippingAddress.DistrictName,
                    WardsId = customer.StandardShippingAddress.WardsId,
                    WardsName = customer.StandardShippingAddress.WardsName,
                    DetailAddress = customer.StandardShippingAddress.DetailAddress
                },
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
                    StandardShippingAddress = new ShippingAddressDto
                    {
                        ProvinceId = customer.StandardShippingAddress.ProvinceId,
                        ProvinceCode = customer.StandardShippingAddress.ProvinceCode,
                        ProvinceName = customer.StandardShippingAddress.ProvinceName,
                        DistrictId = customer.StandardShippingAddress.DistrictId,
                        DistrictValue = customer.StandardShippingAddress.DistrictValue,
                        DistrictName = customer.StandardShippingAddress.DistrictName,
                        WardsId = customer.StandardShippingAddress.WardsId,
                        WardsName = customer.StandardShippingAddress.WardsName,
                        DetailAddress = customer.StandardShippingAddress.DetailAddress
                    },
                    PhoneNumber = customer.PhoneNumber,
                    AvtURL = customer.AvtURL,
                    Email = customer.Email
                });
            }

            return customerDtos;
        }

        public async Task LogoutCurrentDeviceAsync(string token)
        {
            await _jwtTokenService.RevokeTokenAsync(token);
        }

        public async Task LogoutAllOtherDevicesAsync(string userId, string currentTokenJti)
        {
            await _jwtTokenService.RevokeAllTokensExceptCurrentAsync(userId, currentTokenJti);
        }

        private string GenerateOtp()
        {
            var bytes = new byte[4]; // Changed from 3 to 4 bytes
            RandomNumberGenerator.Fill(bytes);
            return (BitConverter.ToUInt32(bytes, 0) % 1000000).ToString("D6"); // Ensure 6-digit OTP
        }
    }
}