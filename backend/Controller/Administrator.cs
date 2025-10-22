using Backend.Model.dto.Administrator;
using Backend.Model.dto.Customer;
using Backend.Service.AdministratorService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/administrators")]
    public class AdministratorController : ControllerBase
    {
        private readonly IAdministratorService _administratorService;

        public AdministratorController(IAdministratorService administratorService)
        {
            _administratorService = administratorService ?? throw new ArgumentNullException(nameof(administratorService));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdministrator([FromBody] CreateAdministrator createAdministrator)
        {
            await _administratorService.CreateAdministratorAsync(createAdministrator);
            return Created($"api/administrators/{createAdministrator.Username}", new
            {
                Message = "Tạo tài khoản quản trị viên thành công",
                Data = new { Username = createAdministrator.Username }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginAdministrator loginAdministrator)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var response = await _administratorService.LoginAsync(loginAdministrator, clientIp);
            return Ok(new
            {
                Message = "Đăng nhập thành công",
                Data = response
            });
        }

        [HttpDelete("{username}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteAdministrator(string username)
        {
            await _administratorService.DeleteAdministratorByUsernameAsync(username);
            return Ok(new { Message = "Xóa tài khoản quản trị viên thành công" });
        }

        [HttpDelete("me")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteCurrentAdministrator()
        {
            var administratorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng"));

            await _administratorService.DeleteAdministratorAsync(administratorId);
            return Ok(new { Message = "Xóa tài khoản thành công" });
        }

        [HttpPut("{username}/password")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangePassword(string username, [FromBody] ChangePasswordRequest request)
        {
            await _administratorService.ChangePasswordByUsernameAsync(username, request);
            return Ok(new { Message = "Đổi mật khẩu thành công" });
        }

        [HttpPut("password")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeCurrentPassword([FromBody] ChangePasswordRequest request)
        {
            var administratorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng"));

            await _administratorService.ChangePasswordAsync(administratorId, request);
            return Ok(new { Message = "Đổi mật khẩu thành công" });
        }

        [HttpGet("{username}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetAdministratorInfo(string username)
        {
            var administratorInfo = await _administratorService.GetAdministratorInfoByUsernameAsync(username);
            return Ok(new
            {
                Message = "Lấy thông tin thành công",
                Data = administratorInfo
            });
        }

        [HttpGet("me")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetCurrentAdministratorInfo()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng");

            var administratorInfo = await _administratorService.GetAdministratorInfoByTokenAsync(userIdClaim);
            return Ok(new
            {
                Message = "Lấy thông tin thành công",
                Data = administratorInfo
            });
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetAllAdministrators()
        {
            var administrators = await _administratorService.GetAllAdministratorsAsync();
            return Ok(new
            {
                Message = "Lấy danh sách quản trị viên thành công",
                Data = administrators
            });
        }

        [HttpGet("customers")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _administratorService.GetAllCustomersAsync();
            return Ok(new
            {
                Message = "Lấy danh sách khách hàng thành công",
                Data = customers
            });
        }

        [HttpPost("logout")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> LogoutCurrentDevice()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Không tìm thấy token");

            await _administratorService.LogoutCurrentDeviceAsync(token);
            return Ok(new { Message = "Đăng xuất thành công" });
        }

        [HttpPost("logout/other-devices")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> LogoutAllOtherDevices()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng");

            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(token))
                throw new ArgumentException("Token không hợp lệ");

            var jwtToken = tokenHandler.ReadJwtToken(token);
            var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value 
                ?? throw new ArgumentException("Token không chứa JTI");

            await _administratorService.LogoutAllOtherDevicesAsync(userIdClaim, jtiClaim);
            return Ok(new { Message = "Đăng xuất các thiết bị khác thành công" });
        }
    }
}