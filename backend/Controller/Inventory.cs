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
    }
}