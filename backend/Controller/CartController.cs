// Backend/Controllers/CartController.cs
using Backend.Model.dto.CartDtos;
using Backend.Service.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/cart")]
    [Authorize(Roles = "Customer")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddToCartRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _cartService.AddToCartAsync(dto);

            return result.Success
                ? Ok(new { Message = result.Message })
                : BadRequest(new { Message = result.Message });
        }

        [HttpPut("{cartItemId}")]
        public async Task<IActionResult> Update(long cartItemId, [FromBody] UpdateCartItemRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _cartService.UpdateCartItemAsync(cartItemId, dto);

            return result.Success
                ? Ok(new { Message = "Cập nhât giỏ hàng thành công" })
                : BadRequest(new { Message = result.Message });
        }

        [HttpDelete]
        public async Task<IActionResult> Clear()
        {
            var result = await _cartService.ClearCartAsync();

            return Ok(new { Message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var cart = await _cartService.GetCartAsync();

            return Ok(new
            {
                Message = "Lấy giỏ hàng thành công",
                Data = cart
            });
        }
    }
}