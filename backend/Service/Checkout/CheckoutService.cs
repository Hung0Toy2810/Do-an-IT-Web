using Backend.Model.Entity;
using Backend.Model.dto.CartDtos;
using Backend.Service.Cart;
using Backend.Service.Product;
using Backend.Service.Payment;
using Backend.Repository.InvoiceRepository;
using Backend.Repository.InvoiceDetailRepository;
using Backend.Service.Stock;
using Backend.Service.Shipping;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Backend.Service.Checkout
{
    public interface ICheckoutService
    {
        Task<CheckoutResult> ProcessCheckoutAsync(CheckoutRequest req);
        Task HandleVNPayIPNSuccessAsync(long invoiceId);
    }

    public class CheckoutService : ICheckoutService
    {
        private readonly ICartService _cartService;
        private readonly IProductStockService _stockService;
        private readonly IStockAllocationService _allocationService;
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly IInvoiceDetailRepository _detailRepo;
        private readonly IVNPayService _vnpayService;
        private readonly IShippingService _shippingService;
        private readonly IHttpContextAccessor _httpContext;

        public CheckoutService(
            ICartService cartService,
            IProductStockService stockService,
            IStockAllocationService allocationService,
            IInvoiceRepository invoiceRepo,
            IInvoiceDetailRepository detailRepo,
            IVNPayService vnpayService,
            IShippingService shippingService,
            IHttpContextAccessor httpContext)
        {
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
            _allocationService = allocationService ?? throw new ArgumentNullException(nameof(allocationService));
            _invoiceRepo = invoiceRepo ?? throw new ArgumentNullException(nameof(invoiceRepo));
            _detailRepo = detailRepo ?? throw new ArgumentNullException(nameof(detailRepo));
            _vnpayService = vnpayService ?? throw new ArgumentNullException(nameof(vnpayService));
            _shippingService = shippingService ?? throw new ArgumentNullException(nameof(shippingService));
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        }

        public async Task<CheckoutResult> ProcessCheckoutAsync(CheckoutRequest req)
        {
            var customerId = GetCustomerId();
            var cart = await _cartService.GetCartAsync();
            if (!cart.Items.Any()) return Fail("Giỏ hàng trống");

            var orderId = $"ORD-{DateTime.Now:yyyyMMddHHmmss}-{customerId:N}".Substring(0, 32);

            // 1. KIỂM TRA STOCK
            foreach (var item in cart.Items)
            {
                if (item.Quantity > item.AvailableStock)
                    return Fail($"Sản phẩm {item.ProductName} chỉ còn {item.AvailableStock}");
            }

            // 2. RESERVE STOCK
            foreach (var item in cart.Items)
            {
                await _stockService.ReserveStockAsync(
                    productSlug: item.ProductSlug,
                    variantSlug: item.VariantSlug,
                    quantity: item.Quantity,
                    orderId: orderId
                );
            }

            // 3. TÍNH TIỀN
            var subtotal = cart.Subtotal;
            var shippingFee = await _shippingService.CalculateFeeAsync(req.Address);
            var total = subtotal + shippingFee;

            // 4. TẠO INVOICE
            var invoice = new Invoice
            {
                CustomerId = customerId,
                TotalAmount = total,
                PaymentMethod = req.PaymentMethod,
                Status = (int)InvoiceStatus.Pending,
                ShippingAddress = req.Address,
                TrackingCode = orderId,
                ReceiverName = req.ReceiverName,
                ReceiverPhone = req.ReceiverPhone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Carrier = "viettelpost"
            };

            var invoiceId = await _invoiceRepo.CreateInvoiceAsync(invoice);
            if (invoiceId <= 0)
            {
                await _stockService.ReleaseStockAsync(orderId);
                return Fail("Tạo hóa đơn thất bại");
            }

            // 5. TẠO + LƯU + GÁN BATCH ID TỪNG DETAIL
            var details = new List<InvoiceDetail>();
            foreach (var item in cart.Items)
            {
                var detail = new InvoiceDetail
                {
                    InvoiceId = invoiceId,
                    ProductId = item.ProductId,
                    VariantSlug = item.VariantSlug,
                    Quantity = item.Quantity,
                    Price = item.DiscountedPrice,
                    ShipmentBatchId = null
                };
                details.Add(detail);
            }

            await _detailRepo.AddRangeAsync(details);

            foreach (var detail in details)
            {
                var batchId = await _allocationService.AllocateFromBatchAsync(
                    productId: detail.ProductId,
                    variantSlug: detail.VariantSlug,
                    quantity: detail.Quantity,
                    invoiceDetailId: detail.Id
                );

                if (batchId <= 0)
                {
                    await RollbackAsync(orderId, invoiceId);
                    return Fail("Không đủ hàng trong kho (lô)");
                }

                var updated = await _detailRepo.UpdateShipmentBatchIdAsync(detail.Id, batchId);
                if (!updated)
                {
                    await RollbackAsync(orderId, invoiceId);
                    return Fail("Cập nhật lô hàng thất bại");
                }
            }

            // XÓA GIỎ HÀNG NGAY SAU KHI TẠO HÓA ĐƠN THÀNH CÔNG
            await _cartService.ClearCartAsync();

            // 6. XỬ LÝ THEO PHƯƠNG THỨC THANH TOÁN
            if (req.PaymentMethod == "COD")
            {
                var shipment = await _shippingService.CreateShipmentAsync(invoiceId, total);
                if (!shipment.Success || string.IsNullOrEmpty(shipment.TrackingNumber))
                {
                    await RollbackAsync(orderId, invoiceId);
                    return Fail("Tạo vận đơn thất bại");
                }

                await FinalizeSuccessAsync(orderId, invoiceId, shipment.TrackingNumber);
                return Success(invoiceId, null, shipment.TrackingNumber);
            }
            else // VNPAY
            {
                var ip = _httpContext.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                var paymentResult = await _vnpayService.CreatePaymentUrlAsync(invoiceId, total, ip);

                if (!paymentResult.IsValid)
                {
                    await RollbackAsync(orderId, invoiceId);
                    return Fail(paymentResult.ErrorMessage ?? "Tạo link thanh toán thất bại");
                }

                return new CheckoutResult
                {
                    Success = true,
                    InvoiceId = invoiceId,
                    PaymentUrl = paymentResult.PaymentUrl,
                    RequiresPayment = true
                };
            }
        }

        public async Task HandleVNPayIPNSuccessAsync(long invoiceId)
        {
            var invoice = await _invoiceRepo.GetInvoiceByIdAsync(invoiceId, includeDetails: false, includePayment: true);
            if (invoice == null || invoice.Status != (int)InvoiceStatus.Pending) return;

            var orderId = invoice.TrackingCode;

            var shipment = await _shippingService.CreateShipmentAsync(invoiceId, 0);
            if (!shipment.Success || string.IsNullOrEmpty(shipment.TrackingNumber))
            {
                await _invoiceRepo.UpdateInvoiceStatusAsync(invoiceId, (int)InvoiceStatus.PaymentFailed);
                await _stockService.ReleaseStockAsync(orderId);
                return;
            }

            // CHỈ XÁC NHẬN STOCK VÀ CẬP NHẬT TRẠNG THÁI
            await _stockService.ConfirmStockReservationAsync(orderId);
            await _invoiceRepo.UpdateInvoiceStatusAsync(invoiceId, (int)InvoiceStatus.Paid);

            // SỬA: DÙNG UpdateInvoiceStatusAsync ĐỂ CẬP NHẬT TrackingCode
            await _invoiceRepo.UpdateTrackingCodeAsync(invoiceId, shipment.TrackingNumber);
        }

        private async Task FinalizeSuccessAsync(string orderId, long invoiceId, string trackingNumber)
        {
            await _stockService.ConfirmStockReservationAsync(orderId);
            await _invoiceRepo.UpdateInvoiceStatusAsync(invoiceId, (int)InvoiceStatus.Paid);
            await _invoiceRepo.UpdateTrackingCodeAsync(invoiceId, trackingNumber);
        }

        private async Task RollbackAsync(string orderId, long invoiceId)
        {
            await _stockService.ReleaseStockAsync(orderId);

            var details = await _detailRepo.GetByInvoiceIdAsync(invoiceId);
            foreach (var d in details)
                await _allocationService.ReleaseFromBatchAsync(d.Id);

            await _detailRepo.DeleteByInvoiceIdAsync(invoiceId);

            // SỬA: DÙNG UpdateInvoiceStatusAsync ĐỂ XÓA (hoặc tạo phương thức Delete riêng)
            // Ở đây dùng UpdateInvoiceStatusAsync để đánh dấu Cancelled
            await _invoiceRepo.UpdateInvoiceStatusAsync(invoiceId, (int)InvoiceStatus.Cancelled);
        }

        private Guid GetCustomerId()
        {
            var claim = _httpContext.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !Guid.TryParse(claim.Value, out var id))
                throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng");
            return id;
        }

        private CheckoutResult Fail(string msg) => new() { Success = false, Message = msg };
        private CheckoutResult Success(long invoiceId, string? url, string tracking) => new()
        {
            Success = true,
            InvoiceId = invoiceId,
            PaymentUrl = url,
            TrackingNumber = tracking
        };
    }

    // === DTO & ENUM ===
    public class CheckoutRequest
    {
        public string PaymentMethod { get; set; } = "COD";
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public ShippingAddress Address { get; set; } = new();
    }

    public class CheckoutResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public long InvoiceId { get; set; }
        public string? PaymentUrl { get; set; }
        public bool RequiresPayment { get; set; }
        public string? TrackingNumber { get; set; }
    }

    public enum InvoiceStatus
    {
        Pending = 0,
        Paid = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4,
        PaymentFailed = 5
    }
}