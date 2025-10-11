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

        /// <summary>
        /// Gửi yêu cầu sinh OTP để đăng ký tài khoản.
        /// </summary>
        /// <param name="request">Số điện thoại để sinh OTP.</param>
        /// <returns>Phản hồi cho biết OTP đã được sinh.</returns>
        [HttpPost("register/otp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateOtpForRegistration([FromBody] GenerateOtpRequest request)
        {
            await _customerService.GenerateOtpForRegistrationAsync(request.PhoneNumber);
            return Ok(new { Message = "OTP đã được sinh và in ra console." });
        }

        /// <summary>
        /// Tạo tài khoản khách hàng sau khi xác thực OTP.
        /// </summary>
        /// <param name="request">Dữ liệu khách hàng và OTP.</param>
        /// <returns>Phản hồi cho biết kết quả tạo tài khoản.</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerWithOtpRequest request)
        {
            await _customerService.CreateCustomerAsync(request.CreateCustomer, request.Otp);
            return Created($"api/customers/{request.CreateCustomer.PhoneNumber}", new
            {
                Message = "Tạo tài khoản thành công.",
                PhoneNumber = request.CreateCustomer.PhoneNumber
            });
        }

        /// <summary>
        /// Đăng nhập khách hàng với số điện thoại và mật khẩu.
        /// </summary>
        /// <param name="loginCustomer">Dữ liệu đăng nhập gồm số điện thoại và mật khẩu.</param>
        /// <returns>Phản hồi chứa token JWT và thời gian hết hạn.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCustomer loginCustomer)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var response = await _customerService.LoginAsync(loginCustomer, clientIp);
            return Ok(response);
        }

        /// <summary>
        /// Gửi yêu cầu sinh OTP để khôi phục mật khẩu.
        /// </summary>
        /// <param name="request">Số điện thoại để sinh OTP.</param>
        /// <returns>Phản hồi cho biết OTP đã được sinh.</returns>
        [HttpPost("password/reset/otp")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GenerateOtpForPasswordRecovery([FromBody] GenerateOtpRequest request)
        {
            await _customerService.GenerateOtpForPasswordRecoveryAsync(request.PhoneNumber);
            return Ok(new { Message = "OTP cho khôi phục mật khẩu đã được sinh và in ra console." });
        }

        /// <summary>
        /// Đặt lại mật khẩu sau khi xác thực OTP.
        /// </summary>
        /// <param name="request">Số điện thoại, OTP và mật khẩu mới.</param>
        /// <returns>Phản hồi cho biết kết quả đặt lại mật khẩu.</returns>
        [HttpPost("password/reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _customerService.ResetPasswordAsync(request.PhoneNumber, request.Otp, request.NewPassword);
            return Ok(new { Message = "Đặt lại mật khẩu thành công." });
        }

        /// <summary>
        /// Cập nhật ảnh đại diện của khách hàng.
        /// </summary>
        /// <returns>URL công khai của ảnh đại diện đã cập nhật.</returns>
        [HttpPut("avatar")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAvatar([FromForm] IFormFile file)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng trong token.");
            }
            var customerId = Guid.Parse(userIdClaim); // Let exception propagate if invalid
            var avatarUrl = await _customerService.UpdateAvatarAsync(customerId, file);
            return Ok(new { Message = "Cập nhật ảnh đại diện thành công.", AvatarUrl = avatarUrl });
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng (CustomerName, StandardShippingAddress, Email).
        /// </summary>
        /// <param name="request">Dữ liệu khách hàng cần cập nhật.</param>
        /// <returns>Phản hồi cho biết kết quả cập nhật.</returns>
        [HttpPut]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCustomer([FromBody] UpdateCustomerRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng trong token.");
            }
            var customerId = Guid.Parse(userIdClaim); // Let exception propagate if invalid
            await _customerService.UpdateCustomerAsync(customerId, request);
            return Ok(new { Message = "Cập nhật thông tin khách hàng thành công." });
        }

        /// <summary>
        /// Xóa tài khoản khách hàng bằng cách đặt trạng thái thành không hoạt động.
        /// </summary>
        /// <returns>Phản hồi cho biết kết quả xóa tài khoản.</returns>
        [HttpDelete]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteCustomer()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng trong token.");
            }
            var customerId = Guid.Parse(userIdClaim); // Let exception propagate if invalid
            await _customerService.DeleteCustomerAsync(customerId);
            return Ok(new { Message = "Tài khoản khách hàng đã được hủy kích hoạt thành công." });
        }

        /// <summary>
        /// Thay đổi mật khẩu của khách hàng.
        /// </summary>
        /// <param name="request">Dữ liệu mật khẩu cũ và mới.</param>
        /// <returns>Phản hồi cho biết kết quả thay đổi mật khẩu.</returns>
        [HttpPut("password")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng trong token.");
            }
            var customerId = Guid.Parse(userIdClaim); // Let exception propagate if invalid
            await _customerService.ChangePasswordAsync(customerId, request);
            return Ok(new { Message = "Thay đổi mật khẩu thành công." });
        }

        /// <summary>
        /// Đăng xuất khỏi thiết bị hiện tại.
        /// </summary>
        /// <returns>Phản hồi cho biết kết quả đăng xuất.</returns>
        [HttpPost("logout")]
        [AllowAnonymous] // Giữ để test, sau khi fix sẽ thêm [Authorize]
        public async Task<IActionResult> LogoutCurrentDevice()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            await _customerService.LogoutCurrentDeviceAsync(token);
            return Ok(new { Message = "Đăng xuất thành công khỏi thiết bị hiện tại." });
        }

        /// <summary>
        /// Đăng xuất khỏi tất cả các thiết bị khác trừ thiết bị hiện tại.
        /// </summary>
        /// <returns>Phản hồi cho biết kết quả đăng xuất.</returns>
        [HttpPost("logout/other-devices")]
        [Authorize(Roles = "Customer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogoutAllOtherDevices()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng trong token.");
            }
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token); // Let exception propagate if invalid
            var jtiClaim = jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value; // Let exception propagate if missing
            await _customerService.LogoutAllOtherDevicesAsync(userIdClaim, jtiClaim);
            return Ok(new { Message = "Đăng xuất thành công khỏi tất cả các thiết bị khác." });
        }

        /// <summary>
        /// Lấy thông tin khách hàng hiện tại dựa trên token JWT.
        /// </summary>
        /// <returns>Thông tin khách hàng hiện tại.</returns>
        [HttpGet("me")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetCurrentCustomerInfo()
        {
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<CustomerController>>();
            var claims = User.Claims.Select(c => $"{c.Type}={c.Value}");
            logger.LogDebug("User claims in /me: {Claims}", string.Join(", ", claims));

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                logger.LogWarning("No NameIdentifier claim found in token.");
                return Unauthorized("Không tìm thấy thông tin người dùng trong token.");
            }

            var customerInfo = await _customerService.GetCustomerInfoByTokenAsync(userIdClaim);
            return Ok(customerInfo);
        }
    }
}