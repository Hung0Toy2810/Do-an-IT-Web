using Backend.Model.dto.Product;
using Backend.Service.Product;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            var productId = await _productService.CreateProductAsync(dto);
            return Created($"api/products/{productId}", new
            {
                Message = "Tạo sản phẩm thành công",
                Data = new { ProductId = productId }
            });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] ProductSearchRequestDto request)
        {
            var result = await _productService.SearchProductsAsync(request);
            return Ok(new
            {
                Message = "Tìm kiếm sản phẩm thành công",
                Data = result
            });
        }

        [HttpGet("subcategory/{slug}")]
        public async Task<IActionResult> GetProductsBySubCategory(
            string slug,
            [FromQuery] string? brand = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? sortByPriceAscending = null)
        {
            var request = new SubCategoryProductRequestDto
            {
                SubCategorySlug = slug,
                Brand = brand,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortByPriceAscending = sortByPriceAscending
            };

            var result = await _productService.GetProductsBySubCategoryAsync(request);
            return Ok(new
            {
                Message = "Lấy danh sách sản phẩm theo danh mục phụ thành công",
                Data = result
            });
        }

        [HttpGet("subcategory/{slug}/brands")]
        public async Task<IActionResult> GetBrandsBySubCategory(string slug)
        {
            var result = await _productService.GetBrandsBySubCategoryAsync(slug);
            return Ok(new
            {
                Message = "Lấy danh sách thương hiệu theo danh mục phụ thành công",
                Data = result
            });
        }

        [HttpPost("filter")]
        public async Task<IActionResult> GetProductsWithAdvancedFilters([FromBody] ProductFilterRequestDto request)
        {
            var result = await _productService.GetProductsWithAdvancedFiltersAsync(request);
            return Ok(new
            {
                Message = "Lọc sản phẩm nâng cao thành công",
                Data = result
            });
        }

        [HttpGet("{productId:long}")]
        public async Task<IActionResult> GetProductDetailById(long productId)
        {
            var product = await _productService.GetProductDetailByIdAsync(productId);
            return Ok(new
            {
                Message = "Lấy chi tiết sản phẩm thành công",
                Data = product
            });
        }

        [HttpGet("slug/{productSlug}")]
        public async Task<IActionResult> GetProductDetailBySlug(string productSlug)
        {
            var product = await _productService.GetProductDetailBySlugAsync(productSlug);
            return Ok(new
            {
                Message = "Lấy chi tiết sản phẩm thành công",
                Data = product
            });
        }

        [HttpGet("card/{productId:long}")]
        public async Task<IActionResult> GetProductCardById(long productId)
        {
            var productCard = await _productService.GetProductCardByIdAsync(productId);
            return Ok(new
            {
                Message = "Lấy thông tin product card thành công",
                Data = productCard
            });
        }

        [HttpGet("card/slug/{productSlug}")]
        public async Task<IActionResult> GetProductCardBySlug(string productSlug)
        {
            var productCard = await _productService.GetProductCardBySlugAsync(productSlug);
            return Ok(new
            {
                Message = "Lấy thông tin product card thành công",
                Data = productCard
            });
        }

        [HttpGet("{productId:long}/variants/{variantSlug}")]
        public async Task<IActionResult> GetVariantInfoById(long productId, string variantSlug)
        {
            var variantInfo = await _productService.GetVariantInfoAsync(productId, variantSlug);
            return Ok(new
            {
                Message = "Lấy thông tin biến thể thành công",
                Data = variantInfo
            });
        }

        [HttpGet("slug/{productSlug}/variants/{variantSlug}")]
        public async Task<IActionResult> GetVariantInfoBySlug(string productSlug, string variantSlug)
        {
            var variantInfo = await _productService.GetVariantInfoAsync(productSlug, variantSlug);
            return Ok(new
            {
                Message = "Lấy thông tin biến thể thành công",
                Data = variantInfo
            });
        }

        [HttpPut("slug/{productSlug}/variants/{variantSlug}/images")]
        public async Task<IActionResult> UpdateVariantImages(string productSlug, string variantSlug, [FromForm] List<IFormFile> images)
        {
            var imageUrls = await _productService.UpdateVariantImagesAsync(productSlug, variantSlug, images);
            return Ok(new
            {
                Message = "Cập nhật hình ảnh biến thể thành công",
                Data = new { ImageUrls = imageUrls }
            });
        }

        [HttpPut("slug/{productSlug}/variants/{variantSlug}/price")]
        public async Task<IActionResult> UpdateVariantPrice(string productSlug, string variantSlug, [FromBody] UpdateVariantPriceRequestDto request)
        {
            await _productService.UpdateVariantPriceAsync(productSlug, variantSlug, request.OriginalPrice, request.DiscountedPrice);
            return Ok(new
            {
                Message = "Cập nhật giá biến thể thành công"
            });
        }

        [HttpPost("bulk-update-prices")]
        public async Task<IActionResult> BulkUpdatePrices([FromBody] List<BulkPriceUpdateDto> updates)
        {
            var result = await _productService.BulkUpdatePricesAsync(updates);
            return Ok(new
            {
                Message = "Cập nhật hàng loạt giá thành công",
                Data = result
            });
        }

        [HttpPatch("slug/{productSlug}/discontinued")]
        public async Task<IActionResult> UpdateIsDiscontinued(string productSlug, [FromBody] UpdateIsDiscontinuedRequestDto request)
        {
            await _productService.UpdateIsDiscontinuedAsync(productSlug, request.IsDiscontinued);
            return Ok(new
            {
                Message = "Cập nhật trạng thái ngừng kinh doanh thành công"
            });
        }
    }
}