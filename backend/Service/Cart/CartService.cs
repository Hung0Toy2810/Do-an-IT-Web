// Backend/Service/Cart/CartService.cs
using Backend.Model.Entity;
using Backend.Model.dto.CartDtos;
using Backend.Model.dto.Product;
using Backend.Repository.CartRepository;
using Backend.Service.Product;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Backend.Model.Nosql;

namespace Backend.Service.Cart
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductService _productService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartService(
            ICartRepository cartRepository,
            IProductService productService,
            IHttpContextAccessor httpContextAccessor)
        {
            _cartRepository = cartRepository;
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
        {
            return product.Variants.FirstOrDefault(v => v.Slug == variantSlug);
        }

        public async Task<CartOperationResultDto> AddToCartAsync(AddToCartRequestDto req)
        {
            if (req.Quantity <= 0)
                return new CartOperationResultDto { Success = false, Message = "Số lượng phải lớn hơn 0" };

            var customerId = CurrentCustomerId;

            var product = await _productService.GetProductDetailByIdAsync(req.ProductId);
            if (product == null)
                return new CartOperationResultDto { Success = false, Message = "Không tìm thấy sản phẩm" };

            var variant = FindVariant(product, req.VariantSlug);
            if (variant == null)
                return new CartOperationResultDto { Success = false, Message = "Không tìm thấy biến thể" };

            if (variant.Stock <= 0)
                return new CartOperationResultDto { Success = false, Message = "Sản phẩm tạm hết hàng" };

            var currentQty = await _cartRepository.GetCurrentQuantityAsync(customerId, req.ProductId, req.VariantSlug);
            var newQty = currentQty + req.Quantity;

            if (newQty > variant.Stock)
            {
                return new CartOperationResultDto
                {
                    Success = false,
                    Message = $"Chỉ còn {variant.Stock} sản phẩm. Vui lòng chọn số lượng ít hơn."
                };
            }

            var cartItem = new Model.Entity.Cart
            {
                CustomerId = customerId,
                ProductId = req.ProductId,
                VariantSlug = req.VariantSlug,
                Quantity = newQty
            };

            await _cartRepository.AddOrUpdateCartItemAsync(cartItem);

            return new CartOperationResultDto
            {
                Success = true,
                Message = "Đã thêm vào giỏ hàng"
            };
        }

        public async Task<CartOperationResultDto> UpdateCartItemAsync(long cartId, UpdateCartItemRequestDto req)
        {
            if (req.Quantity <= 0)
                return new CartOperationResultDto { Success = false, Message = "Số lượng phải lớn hơn 0" };

            var customerId = CurrentCustomerId;
            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartId, customerId);
            if (cartItem == null)
                return new CartOperationResultDto { Success = false, Message = "Không tìm thấy mục trong giỏ" };

            var product = await _productService.GetProductDetailByIdAsync(cartItem.ProductId);
            if (product == null)
                return new CartOperationResultDto { Success = false, Message = "Sản phẩm không tồn tại" };

            var variant = FindVariant(product, cartItem.VariantSlug);
            if (variant == null)
                return new CartOperationResultDto { Success = false, Message = "Biến thể không tồn tại" };

            var finalQty = req.Quantity;
            var message = string.Empty;

            if (finalQty > variant.Stock)
            {
                finalQty = variant.Stock;
                message = $"Số lượng đã giảm còn {variant.Stock} do hết hàng.";
            }

            cartItem.Quantity = finalQty;
            await _cartRepository.AddOrUpdateCartItemAsync(cartItem);

            return new CartOperationResultDto
            {
                Success = true,
                Message = message
            };
        }

        public async Task<CartOperationResultDto> ClearCartAsync()
        {
            var customerId = CurrentCustomerId;
            var items = await _cartRepository.GetAllCartItemsAsync(customerId);
            if (!items.Any())
                return new CartOperationResultDto { Success = true, Message = "Giỏ hàng đã trống" };

            await _cartRepository.RemoveCartItemsAsync(items);
            return new CartOperationResultDto { Success = true, Message = "Đã xóa toàn bộ giỏ hàng" };
        }

        public async Task<GetCartResponseDto> GetCartAsync()
        {
            var customerId = CurrentCustomerId;
            var dbItems = await _cartRepository.GetAllCartItemsAsync(customerId);
            var result = new GetCartResponseDto();

            if (!dbItems.Any()) return result;

            var productIds = dbItems.Select(x => x.ProductId).Distinct().ToList();
            var productTasks = productIds.Select(id => _productService.GetProductDetailByIdAsync(id));
            var products = await Task.WhenAll(productTasks);
            var productDict = products
                .Where(p => p != null)
                .ToDictionary(p => p!.Id, p => p!);

            foreach (var item in dbItems)
            {
                if (!productDict.TryGetValue(item.ProductId, out var product)) continue;

                var variant = FindVariant(product, item.VariantSlug);
                if (variant == null) continue;

                // SỬA LỖI: BỎ QUA NẾU HẾT HÀNG (STOCK = 0)
                if (variant.Stock <= 0)
                {
                    continue;
                }

                var qty = item.Quantity;
                var message = string.Empty;

                if (qty > variant.Stock)
                {
                    qty = variant.Stock;
                    message = $"Số lượng đã giảm còn {variant.Stock} do hết hàng.";
                    item.Quantity = qty;
                    await _cartRepository.AddOrUpdateCartItemAsync(item);
                }

                result.Items.Add(new CartItemDto
                {
                    CartId = item.Id,
                    ProductId = item.ProductId,
                    VariantSlug = item.VariantSlug,
                    Quantity = qty,
                    ProductName = product.Name,
                    ProductSlug = product.Slug,
                    FirstImage = variant.Images?.FirstOrDefault(),
                    Attributes = variant.Attributes,
                    OriginalPrice = variant.OriginalPrice,
                    DiscountedPrice = variant.DiscountedPrice,
                    AvailableStock = variant.Stock,
                    Message = message
                });
            }

            return result;
        }
    }
}