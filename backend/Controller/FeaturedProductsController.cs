// Controllers/FeaturedProductsController.cs
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/featured")]
    public class FeaturedProductsController : ControllerBase
    {
        private readonly IRedisProductViewService _viewService;

        public FeaturedProductsController(IRedisProductViewService viewService)
        {
            _viewService = viewService;
        }

        /// <summary>
        /// Lấy 30 sản phẩm được xem nhiều nhất hôm nay (24h gần nhất)
        /// Cache 15 phút → cực nhanh
        /// </summary>
        [HttpGet("top-30-today")]
        public async Task<ActionResult<List<long>>> GetTop30Today()
        {
            var ids = await _viewService.GetTop30ViewedTodayAsync();
            return Ok(ids);
        }
    }
}