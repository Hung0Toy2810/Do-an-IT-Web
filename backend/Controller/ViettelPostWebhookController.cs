// Backend/Controllers/ViettelPostWebhookController.cs
using Backend.Model.Entity;
using Backend.Service.Shipping;
using Backend.Repository.InvoiceRepository;
using Microsoft.AspNetCore.Mvc;
using Backend.Service.Checkout;
namespace Backend.Controllers
{
    [ApiController]
    [Route("api/webhook/viettelpost")]
    public class ViettelPostWebhookController : ControllerBase
    {
        private readonly IViettelPostWebhookService _webhookService;
        private readonly IInvoiceRepository _invoiceRepo;

        public ViettelPostWebhookController(
            IViettelPostWebhookService webhookService,
            IInvoiceRepository invoiceRepo)
        {
            _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
            _invoiceRepo = invoiceRepo ?? throw new ArgumentNullException(nameof(invoiceRepo));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Receive([FromBody] ViettelPostWebhookDto dto)
        {
            if (string.IsNullOrEmpty(dto.TrackingNumber))
                return BadRequest(new { Message = "Tracking number is required" });

            var invoice = await _invoiceRepo.GetInvoiceByTrackingCodeAsync(dto.TrackingNumber);
            if (invoice == null)
                return NotFound(new { Message = $"Không tìm thấy hóa đơn với mã {dto.TrackingNumber}" });

            var status = dto.Status.ToUpper() switch
            {
                "DELIVERED" => InvoiceStatus.Delivered,
                "CANCELLED" => InvoiceStatus.Cancelled,
                "SHIPPED" => InvoiceStatus.Shipped,
                _ => InvoiceStatus.Pending
            };

            var result = await _webhookService.HandleStatusUpdateAsync(invoice.Id, status, dto.Note);

            return result 
                ? Ok(new { Message = "Webhook processed successfully" })
                : BadRequest(new { Message = "Failed to process webhook" });
        }
    }

    public class ViettelPostWebhookDto
    {
        public string TrackingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}