using Backend.Model.dto.Product;
using Backend.Service.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/product-stock")]
    public class ProductStockController : ControllerBase
    {
        private readonly IProductStockService _productStockService;

        public ProductStockController(IProductStockService productStockService)
        {
            _productStockService = productStockService;
        }

        [HttpPut("increase/{productSlug}/{variantSlug}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> IncreaseStock(string productSlug, string variantSlug, [FromQuery] int quantity)
        {
            var success = await _productStockService.IncreaseStockAsync(productSlug, variantSlug, quantity);
            return success 
                ? Ok(new { Message = "Tăng tồn kho thành công" })
                : BadRequest(new { Message = "Tăng tồn kho thất bại" });
        }

        [HttpPut("decrease/{productSlug}/{variantSlug}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DecreaseStock(string productSlug, string variantSlug, [FromQuery] int quantity)
        {
            var success = await _productStockService.DecreaseStockAsync(productSlug, variantSlug, quantity);
            return success 
                ? Ok(new { Message = "Giảm tồn kho thành công" })
                : BadRequest(new { Message = "Giảm tồn kho thất bại" });
        }

        [HttpPut("set/{productSlug}/{variantSlug}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> SetStock(string productSlug, string variantSlug, [FromQuery] int stock)
        {
            var success = await _productStockService.SetStockAsync(productSlug, variantSlug, stock);
            return success 
                ? Ok(new { Message = "Cập nhật tồn kho thành công" })
                : BadRequest(new { Message = "Cập nhật tồn kho thất bại" });
        }

        [HttpPost("reserve/{productSlug}/{variantSlug}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ReserveStock(string productSlug, string variantSlug, [FromBody] ReserveStockDto dto)
        {
            var success = await _productStockService.ReserveStockAsync(productSlug, variantSlug, dto.Quantity, dto.OrderId, dto.ExpirationMinutes);
            return success 
                ? Ok(new { Message = "Đặt trước tồn kho thành công" })
                : BadRequest(new { Message = "Đặt trước tồn kho thất bại" });
        }

        [HttpPost("release/{orderId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ReleaseStock(string orderId)
        {
            var success = await _productStockService.ReleaseStockAsync(orderId);
            return success 
                ? Ok(new { Message = "Hủy đặt trước tồn kho thành công" })
                : BadRequest(new { Message = "Hủy đặt trước tồn kho thất bại" });
        }

        [HttpPost("confirm/{orderId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ConfirmStockReservation(string orderId)
        {
            var success = await _productStockService.ConfirmStockReservationAsync(orderId);
            return success 
                ? Ok(new { Message = "Xác nhận đặt trước tồn kho thành công" })
                : BadRequest(new { Message = "Xác nhận đặt trước tồn kho thất bại" });
        }

        [HttpGet("available/{productSlug}")]
        public async Task<IActionResult> GetAvailableStockByVariants(string productSlug)
        {
            var stock = await _productStockService.GetAvailableStockByVariantsAsync(productSlug);
            return Ok(new { Message = "Lấy thông tin tồn kho thành công", Data = stock });
        }

        [HttpPut("bulk-update")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> BulkUpdateStock([FromBody] List<BulkStockUpdateDto> updates)
        {
            var result = await _productStockService.BulkUpdateStockAsync(updates);
            return Ok(new { Message = "Cập nhật tồn kho hàng loạt thành công", Data = result });
        }
    }
}