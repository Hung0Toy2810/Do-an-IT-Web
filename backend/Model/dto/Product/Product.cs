namespace Backend.Model.dto.Product
{
    public class CreateProductDocumentDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, List<string>> AttributeOptions { get; set; } = new();
        public List<CreateVariantDto> Variants { get; set; } = new();
    }

    public class CreateVariantDto
    {
        public string Slug { get; set; } = string.Empty;
        public Dictionary<string, string> Attributes { get; set; } = new();
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public List<SpecificationDto> Specifications { get; set; } = new();
    }

    public class SpecificationDto
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class ProductCardDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string? FirstImage { get; set; }
        public decimal MinDiscountedPrice { get; set; }
        public decimal OriginalPriceOfMinVariant { get; set; }
        public bool IsDiscontinued { get; set; }
    }

    public class VariantInfoDto
    {
        public long ProductId { get; set; }
        public string ProductSlug { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? FirstImage { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
    }
}