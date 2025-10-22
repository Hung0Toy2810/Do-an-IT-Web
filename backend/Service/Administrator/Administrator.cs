using Backend.Model.dto.Administrator;
using Backend.Model.dto.Customer;
using System;
using System.Threading.Tasks;
using Backend.Repository.AdministratorRepository;
using Backend.Service.Password;
using Backend.Service.Token;
using Backend.Model.dto;
using Backend.Service.CustomerService;
using System.Linq;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Backend.Service.AdministratorService
{
    public interface IAdministratorService
    {
        Task CreateAdministratorAsync(CreateAdministrator createAdministrator);
        Task<LoginResponse> LoginAsync(LoginAdministrator loginAdministrator, string clientIp);
        Task DeleteAdministratorAsync(Guid administratorId);
        Task DeleteAdministratorByUsernameAsync(string username);
        Task ChangePasswordAsync(Guid administratorId, ChangePasswordRequest request);
        Task ChangePasswordByUsernameAsync(string username, ChangePasswordRequest request);
        Task<AdministratorInfoDto> GetAdministratorInfoAsync(Guid administratorId);
        Task<AdministratorInfoDto> GetAdministratorInfoByTokenAsync(string userIdClaim);
        Task<AdministratorInfoDto> GetAdministratorInfoByUsernameAsync(string username);
        Task<List<AdministratorInfoDto>> GetAllAdministratorsAsync();
        Task<List<CustomerInfoDto>> GetAllCustomersAsync();
        Task LogoutCurrentDeviceAsync(string token);
        Task LogoutAllOtherDevicesAsync(string userId, string currentTokenJti);
    }

    public class AdministratorService : IAdministratorService
    {
        private readonly IAdministratorRepository _administratorRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly SQLServerDbContext _context;
        private readonly ICustomerService _customerService;

        public AdministratorService(
            IAdministratorRepository administratorRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            SQLServerDbContext context,
            ICustomerService customerService)
        {
            _administratorRepository = administratorRepository ?? throw new ArgumentNullException(nameof(administratorRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        public async Task CreateAdministratorAsync(CreateAdministrator createAdministrator)
        {
            if (createAdministrator == null)
                throw new ArgumentNullException(nameof(createAdministrator), "Dữ liệu quản trị viên không được để trống.");

            if (string.IsNullOrWhiteSpace(createAdministrator.Username))
                throw new ArgumentException("Tên đăng nhập không được để trống.", nameof(createAdministrator.Username));

            if (string.IsNullOrWhiteSpace(createAdministrator.Password))
                throw new ArgumentException("Mật khẩu không được để trống.", nameof(createAdministrator.Password));

            if (await _administratorRepository.IsUsernameTakenAsync(createAdministrator.Username))
                throw new InvalidOperationException("Tên đăng nhập đã tồn tại.");

            var administrator = new Administrator
            {
                Username = createAdministrator.Username,
                PasswordHash = _passwordHasher.HashPassword(createAdministrator.Password),
                Status = true
            };

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _administratorRepository.CreateAdministratorAsync(administrator);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginAdministrator loginAdministrator, string clientIp)
        {
            if (loginAdministrator == null)
                throw new ArgumentNullException(nameof(loginAdministrator), "Dữ liệu đăng nhập không được để trống.");

            if (string.IsNullOrWhiteSpace(loginAdministrator.Username))
                throw new ArgumentException("Tên đăng nhập không được để trống.", nameof(loginAdministrator.Username));

            if (string.IsNullOrWhiteSpace(loginAdministrator.Password))
                throw new ArgumentException("Mật khẩu không được để trống.", nameof(loginAdministrator.Password));

            var administrator = await _administratorRepository.GetAdministratorByUsernameAsync(loginAdministrator.Username);
            if (administrator == null)
                throw new UnauthorizedAccessException("Sai tên đăng nhập hoặc mật khẩu.");

            if (!_passwordHasher.VerifyPassword(loginAdministrator.Password, administrator.PasswordHash))
                throw new UnauthorizedAccessException("Sai tên đăng nhập hoặc mật khẩu.");

            if (!administrator.Status)
                throw new UnauthorizedAccessException("Tài khoản quản trị đã bị khóa.");

            var token = await _jwtTokenService.GenerateTokenAsync(
                id: administrator.Id.ToString(),
                username: administrator.Username,
                role: "Administrator",
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

        public async Task DeleteAdministratorAsync(Guid administratorId)
        {
            var administrator = await _administratorRepository.GetAdministratorByIdAsync(administratorId);
            if (administrator == null)
                throw new ArgumentException("Không tìm thấy quản trị viên.");

            if (!administrator.Status)
                throw new InvalidOperationException("Tài khoản quản trị đã bị khóa.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                administrator.Status = false;
                await _administratorRepository.UpdateAdministratorAsync(administrator);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteAdministratorByUsernameAsync(string username)
        {
            var administrator = await _administratorRepository.GetAdministratorByUsernameAsync(username);
            if (administrator == null)
                throw new ArgumentException("Không tìm thấy quản trị viên.");

            if (!administrator.Status)
                throw new InvalidOperationException("Tài khoản quản trị đã bị khóa.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                administrator.Status = false;
                await _administratorRepository.UpdateAdministratorAsync(administrator);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ChangePasswordAsync(Guid administratorId, ChangePasswordRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Yêu cầu đổi mật khẩu không được để trống.");

            if (string.IsNullOrWhiteSpace(request.OldPassword))
                throw new ArgumentException("Mật khẩu cũ không được để trống.", nameof(request.OldPassword));

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new ArgumentException("Mật khẩu mới không được để trống.", nameof(request.NewPassword));

            var administrator = await _administratorRepository.GetAdministratorByIdAsync(administratorId);
            if (administrator == null)
                throw new ArgumentException("Không tìm thấy quản trị viên.");

            if (!administrator.Status)
                throw new UnauthorizedAccessException("Tài khoản quản trị đã bị khóa.");

            if (!_passwordHasher.VerifyPassword(request.OldPassword, administrator.PasswordHash))
                throw new UnauthorizedAccessException("Mật khẩu cũ không đúng.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                administrator.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                await _administratorRepository.UpdateAdministratorAsync(administrator);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ChangePasswordByUsernameAsync(string username, ChangePasswordRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Yêu cầu đổi mật khẩu không được để trống.");

            if (string.IsNullOrWhiteSpace(request.OldPassword))
                throw new ArgumentException("Mật khẩu cũ không được để trống.", nameof(request.OldPassword));

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                throw new ArgumentException("Mật khẩu mới không được để trống.", nameof(request.NewPassword));

            var administrator = await _administratorRepository.GetAdministratorByUsernameAsync(username);
            if (administrator == null)
                throw new ArgumentException("Không tìm thấy quản trị viên.");

            if (!administrator.Status)
                throw new UnauthorizedAccessException("Tài khoản quản trị đã bị khóa.");

            if (!_passwordHasher.VerifyPassword(request.OldPassword, administrator.PasswordHash))
                throw new UnauthorizedAccessException("Mật khẩu cũ không đúng.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                administrator.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
                await _administratorRepository.UpdateAdministratorAsync(administrator);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AdministratorInfoDto> GetAdministratorInfoAsync(Guid administratorId)
        {
            var administrator = await _administratorRepository.GetAdministratorByIdAsync(administratorId);
            if (administrator == null)
                throw new ArgumentException("Không tìm thấy quản trị viên.");

            if (!administrator.Status)
                throw new UnauthorizedAccessException("Tài khoản quản trị đã bị khóa.");

            return new AdministratorInfoDto
            {
                Username = administrator.Username,
                Status = administrator.Status
            };
        }

        public async Task<AdministratorInfoDto> GetAdministratorInfoByTokenAsync(string userIdClaim)
        {
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid administratorId))
                throw new UnauthorizedAccessException("Mã người dùng trong token không hợp lệ.");

            var administrator = await _administratorRepository.GetAdministratorByIdAsync(administratorId);
            if (administrator == null)
                throw new ArgumentException("Không tìm thấy quản trị viên.");

            if (!administrator.Status)
                throw new UnauthorizedAccessException("Tài khoản quản trị đã bị khóa.");

            return new AdministratorInfoDto
            {
                Username = administrator.Username,
                Status = administrator.Status
            };
        }

        public async Task<AdministratorInfoDto> GetAdministratorInfoByUsernameAsync(string username)
        {
            var administrator = await _administratorRepository.GetAdministratorByUsernameAsync(username);
            if (administrator == null)
                throw new ArgumentException("Không tìm thấy quản trị viên.");

            if (!administrator.Status)
                throw new UnauthorizedAccessException("Tài khoản quản trị đã bị khóa.");

            return new AdministratorInfoDto
            {
                Username = administrator.Username,
                Status = administrator.Status
            };
        }

        public async Task<List<AdministratorInfoDto>> GetAllAdministratorsAsync()
        {
            var administrators = await _administratorRepository.GetAllAdministratorsAsync();

            return administrators.Select(a => new AdministratorInfoDto
            {
                Username = a.Username,
                Status = a.Status
            }).ToList();
        }

        public async Task<List<CustomerInfoDto>> GetAllCustomersAsync()
        {
            return await _customerService.GetAllCustomersAsync();
        }

        public async Task LogoutCurrentDeviceAsync(string token)
        {
            await _jwtTokenService.RevokeTokenAsync(token);
        }

        public async Task LogoutAllOtherDevicesAsync(string userId, string currentTokenJti)
        {
            await _jwtTokenService.RevokeAllTokensExceptCurrentAsync(userId, currentTokenJti);
        }
    }
}
