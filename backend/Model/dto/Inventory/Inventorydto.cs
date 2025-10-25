namespace Backend.Model.dto.Inventory
{
    public class ExportedBatchDto
    {
        public string BatchCode { get; set; } = string.Empty;
        public int ExportedQuantity { get; set; }
    }
}