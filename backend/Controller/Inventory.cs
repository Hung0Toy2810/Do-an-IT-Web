using Backend.Model.dto.Inventory;
using Backend.Service.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportStock([FromBody] ImportStockRequestDto request)
        {
            var batchCode = await _inventoryService.ImportStockAsync(
                request.ProductSlug, 
                request.VariantSlug, 
                request.Quantity, 
                request.ImportPrice);
            return Ok(new
            {
                Message = "Nhập hàng thành công",
                Data = new { BatchCode = batchCode }
            });
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportStock([FromBody] ExportStockRequestDto request)
        {
            await _inventoryService.ExportStockAsync(request.BatchCode, request.Quantity);
            return Ok(new
            {
                Message = "Xuất hàng thành công"
            });
        }

        // [HttpGet("batches")]
        // public async Task<IActionResult> GetBatches(
        //     [FromQuery] string productSlug,
        //     [FromQuery] string variantSlug)
        // {
        //     if (string.IsNullOrWhiteSpace(productSlug) || string.IsNullOrWhiteSpace(variantSlug))
        //         return BadRequest(new { Message = "productSlug và variantSlug là bắt buộc" });

        //     try
        //     {
        //         var batches = await _inventoryService.GetBatchesByProductAndVariantAsync(productSlug, variantSlug);
        //         return Ok(new
        //         {
        //             Message = "Lấy danh sách lô hàng thành công",
        //             Data = batches
        //         });
        //     }
        //     catch (NotFoundException ex)
        //     {
        //         return NotFound(new { Message = ex.Message });
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, new { Message = "Lỗi hệ thống", Detail = ex.Message });
        //     }
        // }
        
        [HttpGet("batches/all")]
        public async Task<IActionResult> GetAllBatches(
            [FromQuery] string productSlug,
            [FromQuery] string variantSlug)
        {
            if (string.IsNullOrWhiteSpace(productSlug) || string.IsNullOrWhiteSpace(variantSlug))
                return BadRequest(new { Message = "productSlug và variantSlug là bắt buộc" });

            try
            {
                var batches = await _inventoryService.GetAllBatchesByProductAndVariantAsync(productSlug, variantSlug);
                return Ok(new
                {
                    Message = "Lấy tất cả lô hàng thành công (bao gồm lô đã hết)",
                    Data = batches,
                    Total = batches.Count,
                    InStock = batches.Count(b => b.RemainingQuantity > 0),
                    OutOfStock = batches.Count(b => b.RemainingQuantity == 0)
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống", Detail = ex.Message });
            }
        }
    }
}