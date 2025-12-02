using Backend.Model.dto.Product;

namespace Backend.Model.dto.InvoiceDtos
{
    public class InvoiceDetailItemDto
    {
        public long InvoiceDetailId { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string VariantSlug { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public string? FirstImage { get; set; }
        public List<ProductVariantAttributeDto>? Attributes { get; set; }
    }
}