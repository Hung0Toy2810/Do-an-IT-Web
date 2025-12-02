// Backend/Controllers/AdminCustomerController.cs
using Backend.Service.CustomerAdmin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin/customers")]
    [Authorize(Roles = "Administrator")]
    public class AdminCustomerController : ControllerBase
    {
        private readonly ICustomerAdminService _service;

        public AdminCustomerController(ICustomerAdminService service)
        {
            _service = service;
        }

        /// <summary>
        /// Admin: Xem danh sách người dùng + tìm kiếm + lọc + phân trang
        /// GET /api/admin/customers?search=0909&status=false&page=2
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] bool? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetCustomersAsync(search, status, page, pageSize);
            return Ok(new { Message = "Lấy danh sách người dùng thành công", Data = result });
        }

        /// <summary>
        /// Admin: Xem chi tiết 1 người dùng
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var customer = await _service.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound(new { Message = "Không tìm thấy người dùng" });

            return Ok(new { Message = "Thành công", Data = customer });
        }

        /// <summary>
        /// Admin: Khóa / Mở khóa tài khoản
        /// </summary>
        [HttpPost("{id:guid}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var success = await _service.ToggleStatusAsync(id);
            if (!success)
                return NotFound(new { Message = "Không tìm thấy người dùng" });

            return Ok(new { Message = "Cập nhật trạng thái thành công" });
        }
    }
}