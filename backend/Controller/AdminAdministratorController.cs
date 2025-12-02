// Backend/Controllers/AdminAdministratorController.cs
using Backend.Model.dto.AdministratorAdminDtos;
using Backend.Service.AdministratorAdmin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin/admins")]
    [Authorize(Roles = "Administrator")]
    public class AdminAdministratorController : ControllerBase
    {
        private readonly IAdministratorAdminService _service;

        public AdminAdministratorController(IAdministratorAdminService service)
        {
            _service = service;
        }

        private Guid CurrentAdminId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("Không xác định được admin"));

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] bool? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _service.GetAdministratorsAsync(search, status, page, pageSize);
            return Ok(new { Message = "Lấy danh sách admin thành công", Data = result });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var admin = await _service.GetAdministratorByIdAsync(id);
            if (admin == null) return NotFound();
            return Ok(new { Message = "Thành công", Data = admin });
        }

        [HttpPost("{id:guid}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var success = await _service.ToggleStatusAsync(id, CurrentAdminId);

            if (!success)
            {
                if (id == CurrentAdminId)
                    return BadRequest(new { Message = "Không thể tự khóa tài khoản của chính mình!" });
                return NotFound(new { Message = "Không tìm thấy admin" });
            }

            return Ok(new { Message = "Cập nhật trạng thái thành công" });
        }
    }
}