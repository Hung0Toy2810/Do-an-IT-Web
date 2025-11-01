namespace Backend.Model.dto.Inventory
{
    public class ExportedBatchDto
    {
        public string BatchCode { get; set; } = string.Empty;
        public int ExportedQuantity { get; set; }
    }

    public class ImportStockRequestDto
    {
        public string ProductSlug { get; set; } = string.Empty;
        public string VariantSlug { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal? ImportPrice { get; set; }
    }
    public class ExportStockRequestDto
    {
        public string BatchCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}