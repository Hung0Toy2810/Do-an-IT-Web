using Backend.Service.Cart;
namespace Backend.Model.dto.CartDtos
{
    [ApiController]
    [Route("api/cart")]
    [Authorize(Roles = "Customer")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        public CartController(ICartService cartService) => _cartService = cartService;

        [HttpPost] public async Task<IActionResult> Add([FromBody] AddToCartRequestDto dto)
            => Ok(await _cartService.AddToCartAsync(dto));

        [HttpPut("{cartId}")] public async Task<IActionResult> Update(long cartId, [FromBody] UpdateCartItemRequestDto dto)
            => Ok(await _cartService.UpdateCartItemAsync(cartId, dto));

        [HttpDelete] public async Task<IActionResult> Clear()
            => Ok(await _cartService.ClearCartAsync());

        [HttpGet] public async Task<IActionResult> Get()
            => Ok(await _cartService.GetCartAsync());
    }
}