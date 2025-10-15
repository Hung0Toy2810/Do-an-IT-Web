namespace Backend.Model.dto.Product
{
    public class BulkStockUpdateDto
    {
        public string ProductSlug { get; set; } = string.Empty;
        public string VariantSlug { get; set; } = string.Empty;
        public int StockChange { get; set; }
    }

    public class BulkPriceUpdateDto
    {
        public string ProductSlug { get; set; } = string.Empty;
        public string VariantSlug { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
    }

    public class BulkOperationResultDto
    {
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}