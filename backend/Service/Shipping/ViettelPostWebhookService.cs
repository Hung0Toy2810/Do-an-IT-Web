// // Backend/Service/Shipping/ViettelPostWebhookService.cs
// using Backend.Model.Entity;
// using Backend.Repository.InvoiceRepository;
// using Backend.Service.Product;
// using Backend.Service.Stock;
// using Backend.Repository;
// using Microsoft.Extensions.Logging;
// using Backend.Repository.InvoiceDetailRepository;
// using Backend.Service.Checkout;

// namespace Backend.Service.Shipping
// {
//     public interface IViettelPostWebhookService
//     {
//         Task<bool> HandleStatusUpdateAsync(long invoiceId, InvoiceStatus newStatus, string? note = null);
//     }

//     public class ViettelPostWebhookService : IViettelPostWebhookService
//     {
//         private readonly IInvoiceRepository _invoiceRepo;
//         private readonly IInvoiceDetailRepository _detailRepo;
//         private readonly IProductStockService _stockService;
//         private readonly IStockAllocationService _allocationService;
//         private readonly ILogger<ViettelPostWebhookService> _logger;

//         public ViettelPostWebhookService(
//             IInvoiceRepository invoiceRepo,
//             IInvoiceDetailRepository detailRepo,
//             IProductStockService stockService,
//             IStockAllocationService allocationService,
//             ILogger<ViettelPostWebhookService> logger)
//         {
//             _invoiceRepo = invoiceRepo;
//             _detailRepo = detailRepo;
//             _stockService = stockService;
//             _allocationService = allocationService;
//             _logger = logger;
//         }

//         /// <summary>
//         /// Giả lập webhook từ ViettelPost: cập nhật trạng thái hóa đơn + hành động tương ứng
//         /// </summary>
//         public async Task<bool> HandleStatusUpdateAsync(long invoiceId, InvoiceStatus newStatus, string? note = null)
//         {
//             var invoice = await _invoiceRepo.GetInvoiceByIdAsync(invoiceId, includeDetails: true, includePayment: false);
//             if (invoice == null)
//             {
//                 _logger.LogWarning("Webhook: Không tìm thấy hóa đơn {InvoiceId}", invoiceId);
//                 return false;
//             }

//             var currentStatus = (InvoiceStatus)invoice.Status;
//             if (currentStatus == newStatus)
//             {
//                 _logger.LogInformation("Webhook: Trạng thái không thay đổi: {Status}", newStatus);
//                 return true;
//             }

//             _logger.LogInformation("Webhook: Hóa đơn {InvoiceId} từ {OldStatus} → {NewStatus}", 
//                 invoiceId, currentStatus, newStatus);

//             // CẬP NHẬT TRẠNG THÁI + LỊCH SỬ
//             var success = await _invoiceRepo.UpdateInvoiceStatusAsync(invoiceId, (int)newStatus);
//             if (!success) return false;

//             // HÀNH ĐỘNG THEO TRẠNG THÁI MỚI
//             switch (newStatus)
//             {
//                 case InvoiceStatus.Shipped:
//                     // Đã giao cho shipper → không làm gì thêm
//                     break;

//                 case InvoiceStatus.Delivered:
//                     // Giao thành công → Xác nhận reservation (nếu chưa)
//                     await _stockService.ConfirmStockReservationAsync(invoice.TrackingCode);
//                     break;

//                 case InvoiceStatus.Cancelled:
//                 case InvoiceStatus.PaymentFailed:
//                     // HOÀN HÀNG VÀO KHO + HOÀN LÔ
//                     await ReleaseStockAndBatchesAsync(invoice);
//                     break;

//                 default:
//                     // Các trạng thái khác: Pending, Paid → không làm gì
//                     break;
//             }

//             _logger.LogInformation("Webhook: Xử lý thành công trạng thái {Status} cho hóa đơn {InvoiceId}", 
//                 newStatus, invoiceId);
//             return true;
//         }

//         private async Task ReleaseStockAndBatchesAsync(Invoice invoice)
//         {
//             var orderId = invoice.TrackingCode;

//             // 1. HOÀN STOCK (Mongo)
//             await _stockService.ReleaseStockAsync(orderId);

//             // 2. HOÀN LÔ (SQL)
//             var details = invoice.InvoiceDetails ?? new List<InvoiceDetail>();
//             foreach (var detail in details)
//             {
//                 await _allocationService.ReleaseFromBatchAsync(detail.Id);
//             }

//             _logger.LogInformation("Webhook: Đã hoàn stock + lô cho đơn {OrderId}", orderId);
//         }
//     }

//     // DTO cho webhook (nếu cần)
//     public class ViettelPostWebhookDto
//     {
//         public string TrackingNumber { get; set; } = string.Empty;
//         public string Status { get; set; } = string.Empty;
//         public string? Note { get; set; }
//     }
// }