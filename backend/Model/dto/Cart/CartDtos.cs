using System.Collections.Generic;

namespace Backend.Model.dto.CartDtos
{
    public class AddToCartRequestDto
    {
        public long ProductId { get; set; }
        public string VariantSlug { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemRequestDto
    {
        public int Quantity { get; set; }
    }

    public class CartItemDto
    {
        public long CartId { get; set; }
        public long ProductId { get; set; }
        public string VariantSlug { get; set; } = string.Empty;
        public int Quantity { get; set; }

        // Lấy từ ProductDetailDto.Variants
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? FirstImage { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public int AvailableStock { get; set; }

        public decimal LineTotal => Quantity * DiscountedPrice;
        public string Message { get; set; } = string.Empty;
    }

    public class GetCartResponseDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public int TotalItems => Items.Sum(i => i.Quantity);
        public decimal Subtotal => Items.Sum(i => i.LineTotal);
        public int TotalUniqueProducts => Items.Count;
        public string GeneralMessage { get; set; } = string.Empty;
    }

    public class CartOperationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}