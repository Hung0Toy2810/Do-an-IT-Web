using Backend.Model.dto.Customer;
using Backend.Model.dto;
using Backend.Service.CustomerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        [HttpPost("register/otp")]
        public async Task<IActionResult> GenerateOtpForRegistration([FromBody] GenerateOtpRequest request)
        {
            await _customerService.GenerateOtpForRegistrationAsync(request.PhoneNumber);
            return Ok(new { Message = "OTP đã được gửi" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerWithOtpRequest request)
        {
            await _customerService.CreateCustomerAsync(request.CreateCustomer, request.Otp);
            return Created($"api/customers/{request.CreateCustomer.PhoneNumber}", new
            {
                Message = "Đăng ký thành công",
                Data = new { PhoneNumber = request.CreateCustomer.PhoneNumber }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCustomer loginCustomer)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var response = await _customerService.LoginAsync(loginCustomer, clientIp);
            return Ok(new
            {
                Message = "Đăng nhập thành công",
                Data = response
            });
        }

        [HttpPost("password/reset/otp")]
        public async Task<IActionResult> GenerateOtpForPasswordRecovery([FromBody] GenerateOtpRequest request)
        {
            await _customerService.GenerateOtpForPasswordRecoveryAsync(request.PhoneNumber);
            return Ok(new { Message = "OTP đã được gửi" });
        }

        [HttpPost("password/reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _customerService.ResetPasswordAsync(request.PhoneNumber, request.Otp, request.NewPassword);
            return Ok(new { Message = "Đặt lại mật khẩu thành công" });
        }

        [HttpPut("avatar")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateAvatar([FromForm] IFormFile file)
        {
            var customerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng"));
            
            var avatarUrl = await _customerService.UpdateAvatarAsync(customerId, file);
            return Ok(new
            {
                Message = "Cập nhật ảnh đại diện thành công",
                Data = new { AvatarUrl = avatarUrl }
            });
        }

        [HttpPut]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerRequest request)
        {
            var customerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng"));
            
            await _customerService.UpdateCustomerAsync(customerId, request);
            return Ok(new { Message = "Cập nhật thông tin thành công" });
        }

        [HttpDelete]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> DeleteCustomer()
        {
            var customerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng"));
            
            await _customerService.DeleteCustomerAsync(customerId);
            return Ok(new { Message = "Xóa tài khoản thành công" });
        }

        [HttpPut("password")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var customerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng"));
            
            await _customerService.ChangePasswordAsync(customerId, request);
            return Ok(new { Message = "Đổi mật khẩu thành công" });
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> LogoutCurrentDevice()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            await _customerService.LogoutCurrentDeviceAsync(token);
            return Ok(new { Message = "Đăng xuất thành công" });
        }

        [HttpPost("logout/other-devices")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> LogoutAllOtherDevices()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng");
            
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var jtiClaim = jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
            
            await _customerService.LogoutAllOtherDevicesAsync(userIdClaim, jtiClaim);
            return Ok(new { Message = "Đăng xuất các thiết bị khác thành công" });
        }

        [HttpGet("me")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetCurrentCustomerInfo()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng");
            
            var customerInfo = await _customerService.GetCustomerInfoByTokenAsync(userIdClaim);
            return Ok(new
            {
                Message = "Lấy thông tin thành công",
                Data = customerInfo
            });
        }
    }
}