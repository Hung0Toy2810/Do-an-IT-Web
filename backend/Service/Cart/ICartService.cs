using Backend.Model.dto.CartDtos;

namespace Backend.Service.Cart
{
    public interface ICartService
    {
        Task<CartOperationResultDto> AddToCartAsync(AddToCartRequestDto req);
        Task<CartOperationResultDto> UpdateCartItemAsync(long cartId, UpdateCartItemRequestDto req);
        Task<CartOperationResultDto> ClearCartAsync();
        Task<GetCartResponseDto> GetCartAsync();
    }
}