using Backend.Model.Nosql;
namespace Backend.Model.dto.Product
{
    public class CreateProductDto
    {
        public long SubCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, List<string>> AttributeOptions { get; set; } = new();
        public List<CreateVariantDto> Variants { get; set; } = new();
    }
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

        public float Rating { get; set; } // Thêm rating
        public long TotalRatings { get; set; } // Thêm totalRatings
    }

    public class VariantInfoDto
    {
        public long ProductId { get; set; }
        public string ProductSlug { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? FirstImage { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public float Rating { get; set; } // Thêm rating
        public long TotalRatings { get; set; } // Thêm totalRatings
    }

    public class ProductDetailDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsDiscontinued { get; set; }
        public List<ProductVariant> Variants { get; set; } = new();
        public Dictionary<string, List<string>> AttributeOptions { get; set; } = new();
        public float Rating { get; set; } // Thêm rating
        public long TotalRatings { get; set; } // Thêm totalRatings
    }
    public class ProductSearchRequestDto
    {
        public string Keyword { get; set; } = string.Empty;
        public bool? SortByPriceAscending { get; set; } // null = không sort, true = tăng dần, false = giảm dần
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }

    public class ProductSearchResultDto
    {
        public List<long> ProductIds { get; set; } = new();
        public int TotalCount { get; set; }
    }
    public class SubCategoryProductRequestDto
    {
        public string SubCategorySlug { get; set; } = string.Empty;
        public bool? SortByPriceAscending { get; set; } // null = không sort
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Brand { get; set; } // null = tất cả brand
    }

    public class SubCategoryProductResultDto
    {
        public List<long> ProductIds { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class SubCategoryBrandResultDto
    {
        public List<string> Brands { get; set; } = new();
    }
    public class ProductFilterRequestDto
    {
        public List<string>? Brands { get; set; }
        public List<string>? SubCategorySlugs { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStock { get; set; }
        public bool? OnSale { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } // "price_asc", "price_desc", "newest"
    }
    public class ProductFilterResultDto
    {
        public List<long> ProductIds { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UpdateVariantPriceDto
    {
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
    }

    public class UpdateDiscontinuedDto
    {
        public bool IsDiscontinued { get; set; }
    }

    public class ReserveStockDto
    {
        public int Quantity { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public int ExpirationMinutes { get; set; } = 15;
    }

    public class UpdateVariantPriceRequestDto
    {
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
    }
    public class UpdateIsDiscontinuedRequestDto
    {
        public bool IsDiscontinued { get; set; }
    }
    public class ProductSearchAllRequestDto
    {
        public string? Keyword { get; set; }
        public string? Brand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? SortByPriceAscending { get; set; }
    }
}