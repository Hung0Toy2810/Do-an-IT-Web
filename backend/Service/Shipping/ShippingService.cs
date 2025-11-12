// Backend/Service/Shipping/ShippingService.cs
using Backend.Model.Entity;
using Backend.Model.Nosql;
using Backend.Model.Nosql.ViettelPost;
using Backend.SQLDbContext;
using Backend.Service.ViettelPost;
using Microsoft.EntityFrameworkCore;
using Backend.Service.Product;
namespace Backend.Service.Shipping
{
    public class ShippingService : IShippingService
    {
        private readonly IViettelPostMockService _viettelPostMock;
        private readonly SQLServerDbContext _context;
        private readonly IProductDocumentService _productDocumentService; // Dùng đúng interface

        public ShippingService(
            IViettelPostMockService viettelPostMock,
            SQLServerDbContext context,
            IProductDocumentService productDocumentService)
        {
            _viettelPostMock = viettelPostMock;
            _context = context;
            _productDocumentService = productDocumentService;
        }

        public async Task<decimal> CalculateFeeAsync(ShippingAddress address)
        {
            return 35000m; // Mock phí
        }

        public async Task<ShipmentResult> CreateShipmentAsync(long invoiceId, decimal codAmount)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceDetails)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null) 
                return Fail("Không tìm thấy hóa đơn");

            var items = new List<ViettelPostOrderItem>();

            // LẤY TÊN SẢN PHẨM TỪ MONGO CHO TỪNG CHI TIẾT
            foreach (var detail in invoice.InvoiceDetails)
            {
                var productDoc = await _productDocumentService.GetProductDetailByIdAsync(detail.ProductId);
                var productName = productDoc?.Name ?? $"Sản phẩm #{detail.ProductId}";

                items.Add(new ViettelPostOrderItem
                {
                    ProductName = productName,
                    ProductQuantity = detail.Quantity,
                    ProductPrice = (int)detail.Price,
                    ProductWeight = 500 // gram
                });
            }

            var request = new ViettelPostOrderRequest
            {
                OrderNumber = invoice.TrackingCode,
                SenderFullname = "Shop ABC",
                SenderPhone = "0901234567",
                SenderAddress = "123 Nguyễn Huệ, Quận 1, TP.HCM",
                SenderProvince = 1,
                SenderDistrict = 1,
                SenderWard = 1,

                ReceiverFullname = invoice.ReceiverName,
                ReceiverPhone = invoice.ReceiverPhone,
                ReceiverAddress = invoice.ShippingAddress.DetailAddress ?? "",
                ReceiverProvince = invoice.ShippingAddress.ProvinceId,
                ReceiverDistrict = invoice.ShippingAddress.DistrictId,
                ReceiverWard = invoice.ShippingAddress.WardsId,

                MoneyCollection = (int)codAmount,
                ListItem = items,
                ProductWeight = items.Sum(i => i.ProductWeight * i.ProductQuantity)
            };

            var response = await _viettelPostMock.CreateOrderAsync(request);
            var responseData = response.Data!; // Mock luôn có Data

            if (response.Status != 200)
                return Fail(response.Message ?? "Tạo vận đơn thất bại");

            // CẬP NHẬT TRACKING CODE
            invoice.TrackingCode = responseData.OrderNumber;
            invoice.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ShipmentResult
            {
                Success = true,
                TrackingNumber = responseData.OrderNumber
            };
        }

        private ShipmentResult Fail(string error) => new() { Success = false, Error = error };
    }
}