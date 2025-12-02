// Backend/Controllers/AdminInvoiceController.cs
using Backend.Model.Entity;
using Backend.Service.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Service.Checkout;
namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin/invoices")]
    [Authorize(Roles = "Administrator")]  
    public class AdminInvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public AdminInvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Admin: Lấy danh sách đơn hàng của một khách hàng cụ thể theo CustomerId
        /// GET /api/admin/invoices/customer/{customerId}?page=1&pageSize=20&status=Paid
        /// </summary>
        [HttpGet("customer/{customerId:guid}")]
        public async Task<IActionResult> GetInvoicesByCustomerId(
            Guid customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] InvoiceStatus? status = null)
        {
            var result = await _invoiceService.GetInvoicesByCustomerIdAsync(customerId, page, pageSize, status);

            return Ok(new
            {
                Message = $"Lấy danh sách đơn hàng của khách hàng {customerId} thành công",
                Data = result
            });
        }
        /// <summary>
        /// Admin: Xem tất cả đơn hàng trong hệ thống (mới nhất trước)
        /// GET /api/admin/invoices?page=1&pageSize=20&status=Paid&search=0909
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllInvoices(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] InvoiceStatus? status = null,
            [FromQuery] string? search = null)
        {
            var result = await _invoiceService.GetAllInvoicesForAdminAsync(page, pageSize, status, search);

            return Ok(new
            {
                Message = "Lấy danh sách tất cả đơn hàng thành công",
                Data = result
            });
        }
        [HttpGet("detail/{invoiceId:long}")]
        public async Task<IActionResult> GetInvoiceDetail(long id)
        {
            var result = await _invoiceService.GetInvoiceDetailAsync(id);

            if (result == null)
                return NotFound(new { Message = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập" });

            return Ok(new
            {
                Message = "Lấy chi tiết đơn hàng thành công",
                Data = result
            });
        }
    }
}