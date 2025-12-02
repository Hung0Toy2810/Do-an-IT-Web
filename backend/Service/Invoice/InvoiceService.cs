// Backend/Service/Invoice/InvoiceService.cs
using Backend.Model.Entity;
using Backend.Model.dto.InvoiceDtos;
using Backend.Model.dto.Product;
using Backend.Repository.InvoiceRepository;
using Backend.Repository.InvoiceDetailRepository;
using Backend.Repository.InvoiceStatusHistoryRepository;
using Backend.Service.Product;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Backend.Service.Checkout;
using Backend.Model.Nosql;

namespace Backend.Service.Invoices
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IInvoiceDetailRepository _detailRepository;
        private readonly IInvoiceStatusHistoryRepository _historyRepository;
        private readonly IProductService _productService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            IInvoiceDetailRepository detailRepository,
            IInvoiceStatusHistoryRepository historyRepository,
            IProductService productService,
            IHttpContextAccessor httpContextAccessor)
        {
            _invoiceRepository = invoiceRepository;
            _detailRepository = detailRepository;
            _historyRepository = historyRepository;
            _productService = productService;
            _httpContextAccessor = httpContextAccessor;
        }

        private Guid CurrentCustomerId
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null || !Guid.TryParse(claim.Value, out var id))
                    throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng");
                return id;
            }
        }

        private ProductVariant? FindVariant(ProductDetailDto product, string variantSlug)
            => product.Variants.FirstOrDefault(v => v.Slug == variantSlug);

        public async Task<GetInvoicesResponseDto> GetInvoicesByCustomerAsync(int page = 1, int pageSize = 20, InvoiceStatus? status = null)
        {
            var customerId = CurrentCustomerId;
            var invoices = await _invoiceRepository.GetInvoicesByCustomerIdAsync(customerId, page, pageSize);

            if (status.HasValue)
                invoices = invoices.Where(i => i.Status == (int)status.Value).ToList();

            var totalCount = status.HasValue
                ? invoices.Count
                : await _invoiceRepository.GetInvoicesByCustomerIdAsync(customerId, 1, int.MaxValue)
                    .ContinueWith(t => t.Result.Count);

            var resultInvoices = new List<InvoiceListItemDto>();

            foreach (var invoice in invoices)
            {
                string? firstImage = null;

                if (invoice.InvoiceDetails.Any())
                {
                    var firstItem = invoice.InvoiceDetails.First();
                    var product = await _productService.GetProductDetailByIdAsync(firstItem.ProductId);
                    var variant = product != null ? FindVariant(product, firstItem.VariantSlug) : null;
                    firstImage = variant?.Images?.FirstOrDefault();
                }

                resultInvoices.Add(new InvoiceListItemDto
                {
                    Id = invoice.Id,
                    TrackingCode = invoice.TrackingCode,
                    CreatedAt = invoice.CreatedAt,
                    Status = (InvoiceStatus)invoice.Status,
                    TotalAmount = invoice.TotalAmount,
                    PaymentMethod = invoice.PaymentMethod,
                    TotalItems = invoice.InvoiceDetails.Sum(d => d.Quantity),
                    FirstProductImage = firstImage
                });
            }

            return new GetInvoicesResponseDto
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Invoices = resultInvoices
            };
        }

        public async Task<GetInvoiceDetailResponseDto?> GetInvoiceDetailAsync(long invoiceId)
        {
            var customerId = CurrentCustomerId;
            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId, includeDetails: true, includePayment: true);
            if (invoice == null || invoice.CustomerId != customerId) return null;

            var dbItems = invoice.InvoiceDetails;
            var histories = await _historyRepository.GetByInvoiceIdAsync(invoiceId);

            var productIds = dbItems.Select(x => x.ProductId).Distinct().ToList();
            var products = await Task.WhenAll(productIds.Select(id => _productService.GetProductDetailByIdAsync(id)));
            var productDict = products.Where(p => p != null).ToDictionary(p => p!.Id, p => p!);

            var items = new List<InvoiceDetailItemDto>();

            foreach (var item in dbItems)
            {
                if (!productDict.TryGetValue(item.ProductId, out var product)) continue;
                var variant = FindVariant(product, item.VariantSlug);
                if (variant == null) continue;

                items.Add(new InvoiceDetailItemDto
                {
                    InvoiceDetailId = item.Id,
                    ProductId = item.ProductId,
                    VariantSlug = item.VariantSlug,
                    Quantity = item.Quantity,
                    ProductName = product.Name,
                    ProductSlug = product.Slug,
                    FirstImage = variant.Images?.FirstOrDefault(),
                    Attributes = variant.Attributes?.Select(kvp => new ProductVariantAttributeDto
                    {
                        AttributeName = kvp.Key,
                        AttributeValue = kvp.Value
                    }).ToList() ?? new List<ProductVariantAttributeDto>(),
                    OriginalPrice = variant.OriginalPrice,
                    UnitPrice = item.Price
                });
            }

            return new GetInvoiceDetailResponseDto
            {
                Id = invoice.Id,
                TrackingCode = invoice.TrackingCode,
                CreatedAt = invoice.CreatedAt,
                Status = (InvoiceStatus)invoice.Status,
                TotalAmount = invoice.TotalAmount,
                PaymentMethod = invoice.PaymentMethod,
                ReceiverName = invoice.ReceiverName,
                ReceiverPhone = invoice.ReceiverPhone,
                ShippingAddress = invoice.ShippingAddress,
                Carrier = invoice.Carrier,
                EstimatedDelivery = invoice.EstimatedDelivery,
                StatusHistories = histories.Select(h => new InvoiceStatusHistoryDto
                {
                    Status = Enum.TryParse<InvoiceStatus>(h.Status, out var s) ? s : InvoiceStatus.Pending,
                    CreatedAt = h.CreatedAt,
                    Note = h.Note
                }).OrderBy(h => h.CreatedAt).ToList(),
                Items = items
            };
        }
        public async Task<GetInvoicesResponseDto> GetInvoicesByCustomerIdAsync(
            Guid customerId, 
            int page = 1, 
            int pageSize = 20, 
            InvoiceStatus? status = null)
        {
            var invoices = await _invoiceRepository.GetInvoicesByCustomerIdAsync(customerId, page, pageSize);

            if (status.HasValue)
                invoices = invoices.Where(i => i.Status == (int)status.Value).ToList();

            var totalCount = status.HasValue
                ? invoices.Count
                : await _invoiceRepository.GetInvoicesByCustomerIdAsync(customerId, 1, int.MaxValue)
                    .ContinueWith(t => t.Result.Count);

            var resultInvoices = new List<InvoiceListItemDto>();

            foreach (var invoice in invoices)
            {
                string? firstImage = null;

                if (invoice.InvoiceDetails.Any())
                {
                    var firstItem = invoice.InvoiceDetails.First();
                    var product = await _productService.GetProductDetailByIdAsync(firstItem.ProductId);
                    var variant = product != null ? FindVariant(product, firstItem.VariantSlug) : null;
                    firstImage = variant?.Images?.FirstOrDefault();
                }

                resultInvoices.Add(new InvoiceListItemDto
                {
                    Id = invoice.Id,
                    TrackingCode = invoice.TrackingCode,
                    CreatedAt = invoice.CreatedAt,
                    Status = (InvoiceStatus)invoice.Status,
                    TotalAmount = invoice.TotalAmount,
                    PaymentMethod = invoice.PaymentMethod,
                    TotalItems = invoice.InvoiceDetails.Sum(d => d.Quantity),
                    FirstProductImage = firstImage
                });
            }

            return new GetInvoicesResponseDto
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Invoices = resultInvoices
            };
        }
        public async Task<GetInvoicesResponseDto> GetAllInvoicesForAdminAsync(
            int page = 1,
            int pageSize = 20,
            InvoiceStatus? status = null,
            string? search = null)
        {
            var (invoices, totalCount) = await _invoiceRepository.GetAllInvoicesForAdminAsync(page, pageSize, status, search);

            var items = new List<InvoiceListItemDto>();

            foreach (var invoice in invoices)
            {
                string? firstImage = null;

                if (invoice.InvoiceDetails.Any())
                {
                    var firstDetail = invoice.InvoiceDetails.First();
                    var product = await _productService.GetProductDetailByIdAsync(firstDetail.ProductId);
                    var variant = product != null ? FindVariant(product, firstDetail.VariantSlug) : null;
                    firstImage = variant?.Images?.FirstOrDefault();
                }

                items.Add(new InvoiceListItemDto
                {
                    Id = invoice.Id,
                    TrackingCode = invoice.TrackingCode,
                    CreatedAt = invoice.CreatedAt,
                    Status = (InvoiceStatus)invoice.Status,
                    TotalAmount = invoice.TotalAmount,
                    PaymentMethod = invoice.PaymentMethod,
                    TotalItems = invoice.InvoiceDetails.Sum(d => d.Quantity),
                    FirstProductImage = firstImage,
                    CustomerName = invoice.Customer?.CustomerName ?? "Khách lẻ",
                    CustomerPhone = invoice.Customer?.PhoneNumber ?? "N/A"
                });
            }

            return new GetInvoicesResponseDto
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Invoices = items
            };
        }
        public async Task<GetInvoiceDetailResponseDto?> GetInvoiceDetailForAdminAsync(long invoiceId)
        {
            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId, includeDetails: true, includePayment: true);
            if (invoice == null) return null;

            // Copy nguyên logic map từ GetInvoiceDetailAsync (chỉ bỏ kiểm tra sở hữu)
            var dbItems = invoice.InvoiceDetails;
            var histories = await _historyRepository.GetByInvoiceIdAsync(invoiceId);
            var productIds = dbItems.Select(x => x.ProductId).Distinct().ToList();
            var products = await Task.WhenAll(productIds.Select(id => _productService.GetProductDetailByIdAsync(id)));
            var productDict = products.Where(p => p != null).ToDictionary(p => p!.Id, p => p!);

            var items = new List<InvoiceDetailItemDto>();
            foreach (var item in dbItems)
            {
                if (!productDict.TryGetValue(item.ProductId, out var product)) continue;
                var variant = FindVariant(product, item.VariantSlug);
                if (variant == null) continue;

                items.Add(new InvoiceDetailItemDto
                {
                    InvoiceDetailId = item.Id,
                    ProductId = item.ProductId,
                    VariantSlug = item.VariantSlug,
                    Quantity = item.Quantity,
                    ProductName = product.Name,
                    ProductSlug = product.Slug,
                    FirstImage = variant.Images?.FirstOrDefault(),
                    Attributes = variant.Attributes?.Select(kvp => new ProductVariantAttributeDto
                    {
                        AttributeName = kvp.Key,
                        AttributeValue = kvp.Value
                    }).ToList() ?? new List<ProductVariantAttributeDto>(),
                    OriginalPrice = variant.OriginalPrice,
                    UnitPrice = item.Price
                });
            }

            return new GetInvoiceDetailResponseDto
            {
                Id = invoice.Id,
                TrackingCode = invoice.TrackingCode,
                CreatedAt = invoice.CreatedAt,
                Status = (InvoiceStatus)invoice.Status,
                TotalAmount = invoice.TotalAmount,
                PaymentMethod = invoice.PaymentMethod,
                ReceiverName = invoice.ReceiverName,
                ReceiverPhone = invoice.ReceiverPhone,
                ShippingAddress = invoice.ShippingAddress,
                Carrier = invoice.Carrier,
                EstimatedDelivery = invoice.EstimatedDelivery,
                StatusHistories = histories.Select(h => new InvoiceStatusHistoryDto
                {
                    Status = Enum.TryParse<InvoiceStatus>(h.Status, out var s) ? s : InvoiceStatus.Pending,
                    CreatedAt = h.CreatedAt,
                    Note = h.Note
                }).OrderBy(h => h.CreatedAt).ToList(),
                Items = items
            };
        }
    }
}