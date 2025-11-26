// Backend/Service/Checkout/CheckoutService.cs
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

            try
            {
                // 1. KIỂM TRA STOCK
                foreach (var item in cart.Items)
                {
                    if (item.Quantity > item.AvailableStock)
                        return Fail($"Sản phẩm {item.ProductName} chỉ còn {item.AvailableStock}");
                }

                // 2. TÍNH TIỀN
                var subtotal = cart.Subtotal;
                var shippingFee = await _shippingService.CalculateFeeAsync(req.Address, req.PaymentMethod == "COD");
                var total = subtotal + shippingFee;

                // 3. TẠO INVOICE (Pending)
                var invoice = new Invoice
                {
                    CustomerId = customerId,
                    TotalAmount = total,
                    PaymentMethod = req.PaymentMethod,
                    Status = (int)InvoiceStatus.Pending,
                    ShippingAddress = req.Address,
                    TrackingCode = string.Empty,
                    ReceiverName = req.ReceiverName,
                    ReceiverPhone = req.ReceiverPhone,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Carrier = "viettelpost"
                };

                var invoiceId = await _invoiceRepo.CreateInvoiceAsync(invoice);
                if (invoiceId <= 0) return Fail("Tạo hóa đơn thất bại");

                // 4. TẠO INVOICE DETAILS
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
                        ShipmentBatchId = null // ✅ CHƯA GÁN BATCH
                    };
                    details.Add(detail);
                }

                await _detailRepo.AddRangeAsync(details);

                // 5. ✅ CHỈ RESERVE (trừ Variant.Stock) - CHƯA ALLOCATE
                foreach (var detail in details)
                {
                    var cartItem = cart.Items.First(i => i.ProductId == detail.ProductId && i.VariantSlug == detail.VariantSlug);
                    
                    await _stockService.ReserveStockAsync(
                        productSlug: cartItem.ProductSlug,
                        variantSlug: detail.VariantSlug,
                        quantity: detail.Quantity,
                        invoiceId: invoiceId,
                        invoiceDetailId: detail.Id,
                        expirationMinutes: req.PaymentMethod == "COD" ? 1 : 20
                    );
                }

                await _cartService.ClearCartAsync();

                // 6. XỬ LÝ THEO PHƯƠNG THỨC
                if (req.PaymentMethod == "COD")
                {
                    // ✅ COD: Thanh toán ngay → Allocate + Tạo vận đơn
                    await ProcessPaidInvoiceAsync(invoiceId, total, isCOD: true);

                    var invoice2 = await _invoiceRepo.GetInvoiceByIdAsync(invoiceId);
                    return Success(invoiceId, null, invoice2?.TrackingCode ?? string.Empty);
                }
                else // VNPAY
                {
                    var ip = _httpContext.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                    var paymentResult = await _vnpayService.CreatePaymentUrlAsync(invoiceId, total, ip);

                    if (!paymentResult.IsValid)
                    {
                        await RollbackAsync(invoiceId);
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
            catch (Exception)
            {
                throw;
            }
        }

        // ✅ METHOD MỚI: Xử lý khi thanh toán thành công
        private async Task ProcessPaidInvoiceAsync(long invoiceId, decimal total, bool isCOD)
        {
            try
            {
                // 1. ALLOCATE (trừ kho thật)
                var details = await _detailRepo.GetByInvoiceIdAsync(invoiceId);
                
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
                        throw new BusinessRuleException("Không đủ hàng trong kho (lô)");
                    }

                    await _detailRepo.UpdateShipmentBatchIdAsync(detail.Id, batchId);
                }

                // 2. CONFIRM RESERVATION
                await _stockService.ConfirmStockReservationAsync(invoiceId);

                // 3. TẠO VẬN ĐƠN
                var shipment = await _shippingService.CreateShipmentAsync(invoiceId, isCOD ? total : 0, isCOD);
                if (!shipment.Success || string.IsNullOrEmpty(shipment.TrackingNumber))
                {
                    throw new BusinessRuleException("Tạo vận đơn thất bại");
                }

                // 4. UPDATE INVOICE → PAID
                await _invoiceRepo.UpdateTrackingCodeAsync(invoiceId, shipment.TrackingNumber);
                await _invoiceRepo.UpdateInvoiceStatusAsync(invoiceId, (int)InvoiceStatus.Paid);

                Console.WriteLine("Tạo vận đơn thành công: Invoice {0}, Tracking {1}",
                    invoiceId, shipment.TrackingNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                
                // Rollback nếu allocate/tạo vận đơn thất bại
                await RollbackAsync(invoiceId);
                throw;
            }
        }

        public async Task HandleVNPayIPNSuccessAsync(long invoiceId)
        {
            var invoice = await _invoiceRepo.GetInvoiceByIdAsync(invoiceId, includeDetails: false, includePayment: true);
            if (invoice == null || invoice.Status != (int)InvoiceStatus.Pending) return;

            // ✅ VNPAY thành công → Allocate + Tạo vận đơn
            await ProcessPaidInvoiceAsync(invoiceId, invoice.TotalAmount, isCOD: false);
        }

        // ✅ Rollback - CHỈ release reservation (chưa allocate thì không cần release batch)
        private async Task RollbackAsync(long invoiceId)
        {
            // 1. Giải phóng reservation (cộng lại Variant.Stock)
            await _stockService.ReleaseStockAsync(invoiceId);

            // 2. Giải phóng batch (NẾU đã allocate)
            var details = await _detailRepo.GetByInvoiceIdAsync(invoiceId);
            foreach (var d in details.Where(d => d.ShipmentBatchId.HasValue))
                await _allocationService.ReleaseFromBatchAsync(d.Id);

            // 3. Xóa details + cancel
            await _detailRepo.DeleteByInvoiceIdAsync(invoiceId);
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