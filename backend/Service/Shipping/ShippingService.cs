using Backend.Model.Entity;
using Backend.Model.Nosql;
using Backend.Model.Nosql.ViettelPost;
using Backend.SQLDbContext;
using Backend.Service.ViettelPost;
using Microsoft.EntityFrameworkCore;
using Backend.Service.Product;
using Backend.Service.Shipping;
using Microsoft.Extensions.Logging;
using Backend.Repository.InvoiceRepository;
using Backend.Repository.InvoiceDetailRepository;
using Backend.Service.Stock;
using Backend.Service.Checkout;

namespace Backend.Service.Shipping
{
    public class ShippingService : IShippingService
    {
        private readonly IViettelPostMockService _viettelPostMock;
        private readonly SQLServerDbContext _context;
        private readonly IProductDocumentService _productDocumentService;
        // private readonly IViettelPostWebhookService _webhookService;
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly IInvoiceDetailRepository _detailRepo;
        private readonly IStockAllocationService _allocationService;
        private readonly ILogger<ShippingService> _logger;

        public ShippingService(
            IViettelPostMockService viettelPostMock,
            SQLServerDbContext context,
            IProductDocumentService productDocumentService,
            // IViettelPostWebhookService webhookService,
            IInvoiceRepository invoiceRepo,
            IInvoiceDetailRepository detailRepo,
            IStockAllocationService allocationService,
            ILogger<ShippingService> logger)
        {
            _viettelPostMock = viettelPostMock;
            _context = context;
            _productDocumentService = productDocumentService;
            // _webhookService = webhookService;
            _invoiceRepo = invoiceRepo;
            _detailRepo = detailRepo;
            _allocationService = allocationService;
            _logger = logger;
        }

        public async Task<decimal> CalculateFeeAsync(ShippingAddress address, bool isCOD)
        {
            return 50000m * 1.05m;
        }

        public async Task<ShipmentResult> CreateShipmentAsync(long invoiceId, decimal codAmount, bool isCOD)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceDetails)
                .Include(i => i.ShippingAddress)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null) return Fail("Không tìm thấy hóa đơn");

            var items = new List<ViettelPostOrderItem>();
            int totalWeight = 0;

            foreach (var detail in invoice.InvoiceDetails)
            {
                var productDoc = await _productDocumentService.GetProductDetailByIdAsync(detail.ProductId);
                var name = productDoc?.Name ?? "Sản phẩm";

                var item = new ViettelPostOrderItem
                {
                    ProductName = name,
                    ProductQuantity = detail.Quantity,
                    ProductPrice = (int)detail.Price,
                    ProductWeight = 500
                };
                items.Add(item);
                totalWeight += item.ProductWeight * item.ProductQuantity;
            }

            var request = new ViettelPostOrderRequest
            {
                OrderNumber = invoice.TrackingCode,
                GroupAddressId = 5818802,
                CusId = 722,
                DeliveryDate = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy HH:mm:ss"),
                SenderFullname = "Yanme Shop",
                SenderAddress = "Số 5A ngách 22 ngõ 282 Kim Giang, Đại Kim (0967.363.789), Quận Hoàng Mai, Hà Nội",
                SenderPhone = "0967.363.789",
                SenderEmail = "vanchinh.libra@gmail.com",
                SenderProvince = 1,
                SenderDistrict = 4,
                SenderWard = 0,
                ReceiverFullname = invoice.ReceiverName,
                ReceiverPhone = invoice.ReceiverPhone,
                ReceiverAddress = invoice.ShippingAddress.DetailAddress ?? "",
                ReceiverProvince = invoice.ShippingAddress.ProvinceId,
                ReceiverDistrict = invoice.ShippingAddress.DistrictId,
                ReceiverWard = invoice.ShippingAddress.WardsId,
                ProductName = items.FirstOrDefault()?.ProductName ?? "Sản phẩm",
                ProductQuantity = items.Sum(x => x.ProductQuantity),
                ProductPrice = (int)invoice.TotalAmount,
                ProductWeight = totalWeight,
                ProductType = "HH",
                OrderPayment = isCOD ? 3 : 1,
                OrderService = "VCN",
                OrderNote = "cho xem hàng, không cho thử",
                MoneyCollection = isCOD ? (int)invoice.TotalAmount : 0,
                ListItem = items
            };

            var response = await _viettelPostMock.CreateOrderAsync(request);
            if (response.Status != 200 || response.Data == null)
                return Fail(response.Message ?? "Tạo vận đơn thất bại");

            invoice.TrackingCode = response.Data.OrderNumber;
            invoice.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ShipmentResult
            {
                Success = true,
                TrackingNumber = response.Data.OrderNumber
            };
        }

        // Method mới – DÙNG CHO BACKGROUND SIMULATION (async + note + hoàn kho)
        public async Task SimulateStatusUpdate(long invoiceId, InvoiceStatus newStatus, string? note = null)
        {
            var updated = await _invoiceRepo.UpdateInvoiceStatusAsync(invoiceId, (int)newStatus);
            if (!updated)
            {
                _logger.LogWarning("Không thể cập nhật trạng thái cho đơn {InvoiceId} → {Status}", invoiceId, newStatus);
                return;
            }

            _logger.LogInformation("MÔ PHỎNG → Đơn {InvoiceId} → {Status}", invoiceId, newStatus);

            if (newStatus == InvoiceStatus.Cancelled)
            {
                var details = await _detailRepo.GetByInvoiceIdAsync(invoiceId);
                
                foreach (var detail in details.Where(d => d.ShipmentBatchId.HasValue))
                {
                    var batchId = detail.ShipmentBatchId.Value;
                    var batch = await _context.ShipmentBatches
                        .FirstOrDefaultAsync(b => b.Id == batchId);

                    if (batch != null)
                    {
                        batch.RemainingQuantity += detail.Quantity;
                        _context.ShipmentBatches.Update(batch);

                        _logger.LogWarning(
                            "HOÀN KHO DO GIAO THẤT BẠI → Invoice {InvoiceId} | Detail {DetailId} | " +
                            "Batch {BatchCode} (+{Qty}) → Còn lại {Remain} | Sản phẩm {ProductId}-{Variant}",
                            invoiceId, detail.Id, batch.BatchCode, detail.Quantity, 
                            batch.RemainingQuantity, detail.ProductId, detail.VariantSlug);
                    }
                    else
                    {
                        _logger.LogError("KHÔNG TÌM THẤY LÔ HÀNG ĐỂ HOÀN → BatchId {BatchId} | DetailId {DetailId}", 
                            batchId, detail.Id);
                    }
                }

                // Ghi history hủy đơn
                _context.InvoiceStatusHistories.Add(new InvoiceStatusHistory
                {
                    InvoiceId = invoiceId,
                    Status = "Cancelled",
                    Note = note ?? "Giao hàng không thành công – mô phỏng tự động",
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }
        }

        // Method cũ – DÙNG CHO CHECKOUTSERVICE (fire-and-forget, không hoàn kho)
        public void SimulateStatusUpdateBackground(long invoiceId, InvoiceStatus status)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(status == InvoiceStatus.Shipped ? 5000 : 10000);
                // await _webhookService.HandleStatusUpdateAsync(invoiceId, status);
            });
        }

        private ShipmentResult Fail(string error) => new() { Success = false, Error = error };
    }
}