using Backend.Model.dto.Product;
using Backend.Service.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/product-documents")]
    public class ProductDocumentController : ControllerBase
    {
        private readonly IProductDocumentService _productDocumentService;

        public ProductDocumentController(IProductDocumentService productDocumentService)
        {
            _productDocumentService = productDocumentService;
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDocumentDto dto)
        {
            var product = await _productDocumentService.CreateProductAsync(dto);
            return Created($"api/product-documents/{product.Id}", new { Message = "Tạo sản phẩm thành công", Data = product });
        }

        [HttpGet("{productId:long}")]
        public async Task<IActionResult> GetProductDetailById(long productId)
        {
            var product = await _productDocumentService.GetProductDetailByIdAsync(productId);
            return product == null 
                ? NotFound(new { Message = $"Không tìm thấy sản phẩm với ID {productId}" })
                : Ok(new { Message = "Lấy chi tiết sản phẩm thành công", Data = product });
        }

        [HttpGet("slug/{productSlug}")]
        public async Task<IActionResult> GetProductDetailBySlug(string productSlug)
        {
            var product = await _productDocumentService.GetProductDetailBySlugAsync(productSlug);
            return product == null 
                ? NotFound(new { Message = $"Không tìm thấy sản phẩm với slug {productSlug}" })
                : Ok(new { Message = "Lấy chi tiết sản phẩm thành công", Data = product });
        }

        [HttpGet("card/{productId:long}")]
        public async Task<IActionResult> GetProductCardById(long productId)
        {
            var productCard = await _productDocumentService.GetProductCardByIdAsync(productId);
            return productCard == null 
                ? NotFound(new { Message = $"Không tìm thấy sản phẩm với ID {productId}" })
                : Ok(new { Message = "Lấy thông tin thẻ sản phẩm thành công", Data = productCard });
        }

        [HttpGet("card/slug/{productSlug}")]
        public async Task<IActionResult> GetProductCardBySlug(string productSlug)
        {
            var productCard = await _productDocumentService.GetProductCardBySlugAsync(productSlug);
            return productCard == null 
                ? NotFound(new { Message = $"Không tìm thấy sản phẩm với slug {productSlug}" })
                : Ok(new { Message = "Lấy thông tin thẻ sản phẩm thành công", Data = productCard });
        }

        [HttpGet("variant/{productId:long}/{variantSlug}")]
        public async Task<IActionResult> GetVariantInfoById(long productId, string variantSlug)
        {
            var variantInfo = await _productDocumentService.GetVariantInfoAsync(productId, variantSlug);
            return variantInfo == null 
                ? NotFound(new { Message = $"Không tìm thấy biến thể {variantSlug} cho sản phẩm ID {productId}" })
                : Ok(new { Message = "Lấy thông tin biến thể thành công", Data = variantInfo });
        }

        [HttpGet("variant/slug/{productSlug}/{variantSlug}")]
        public async Task<IActionResult> GetVariantInfoBySlug(string productSlug, string variantSlug)
        {
            var variantInfo = await _productDocumentService.GetVariantInfoAsync(productSlug, variantSlug);
            return variantInfo == null 
                ? NotFound(new { Message = $"Không tìm thấy biến thể {variantSlug} cho sản phẩm {productSlug}" })
                : Ok(new { Message = "Lấy thông tin biến thể thành công", Data = variantInfo });
        }

        [HttpPut("variant/images/{productSlug}/{variantSlug}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateVariantImages(string productSlug, string variantSlug, [FromForm] List<IFormFile> images)
        {
            var imageUrls = await _productDocumentService.UpdateVariantImagesAsync(productSlug, variantSlug, images);
            return Ok(new { Message = "Cập nhật hình ảnh biến thể thành công", Data = imageUrls });
        }

        [HttpPut("variant/price/{productSlug}/{variantSlug}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateVariantPrice(string productSlug, string variantSlug, [FromBody] UpdateVariantPriceDto dto)
        {
            var success = await _productDocumentService.UpdateVariantPriceAsync(productSlug, variantSlug, dto.OriginalPrice, dto.DiscountedPrice);
            return success 
                ? Ok(new { Message = "Cập nhật giá biến thể thành công" })
                : BadRequest(new { Message = "Cập nhật giá biến thể thất bại" });
        }

        [HttpPut("bulk-prices")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> BulkUpdatePrices([FromBody] List<BulkPriceUpdateDto> updates)
        {
            var result = await _productDocumentService.BulkUpdatePricesAsync(updates);
            return Ok(new { Message = "Cập nhật giá hàng loạt thành công", Data = result });
        }

        [HttpPut("discontinued/{productSlug}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateIsDiscontinued(string productSlug, [FromBody] UpdateDiscontinuedDto dto)
        {
            var success = await _productDocumentService.UpdateIsDiscontinuedAsync(productSlug, dto.IsDiscontinued);
            return success 
                ? Ok(new { Message = "Cập nhật trạng thái ngưng kinh doanh thành công" })
                : BadRequest(new { Message = "Cập nhật trạng thái ngưng kinh doanh thất bại" });
        }
    }
}