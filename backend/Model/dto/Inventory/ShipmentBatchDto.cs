namespace Backend.Model.dto.Inventory
{
    public class ShipmentBatchDto
    {
        public string BatchCode { get; set; } = null!;
        public int ImportedQuantity { get; set; }
        public int RemainingQuantity { get; set; }
        public decimal? ImportPrice { get; set; }
        public DateTime ImportedAt { get; set; }
        public string VariantSlug { get; set; } = null!;
    }
}