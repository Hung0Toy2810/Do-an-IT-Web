// Backend/Controllers/InvoiceController.cs
using Backend.Model.Entity;
using Backend.Model.dto.InvoiceDtos;
using Backend.Service.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Service.Checkout;
namespace Backend.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của tôi (phân trang + lọc trạng thái)
        /// GET /api/invoices?page=1&pageSize=10&status=Pending
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyInvoices(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] InvoiceStatus? status = null)
        {
            var result = await _invoiceService.GetInvoicesByCustomerAsync(page, pageSize, status);

            return Ok(new
            {
                Message = "Lấy danh sách đơn hàng thành công",
                Data = result
            });
        }

        /// <summary>
        /// Xem chi tiết đơn hàng
        /// GET /api/invoices/123
        /// </summary>
        [HttpGet("{id:long}")]
        [Authorize(Roles = "Administrator,Customer")]
        public async Task<IActionResult> GetInvoiceDetail(long id)
        {
            GetInvoiceDetailResponseDto? result;

            if (User.IsInRole("Administrator"))
            {
                // Admin → dùng method bỏ qua kiểm tra sở hữu
                result = await _invoiceService.GetInvoiceDetailForAdminAsync(id);
            }
            else
            {
                // Customer → kiểm tra sở hữu như cũ
                result = await _invoiceService.GetInvoiceDetailAsync(id);
            }

            if (result == null)
                return NotFound(new { Message = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập" });

            return Ok(new { Message = "Lấy chi tiết đơn hàng thành công", Data = result });
        }
    }
}