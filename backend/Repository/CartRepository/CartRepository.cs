using Backend.Model.Entity;
using Backend.SQLDbContext;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repository.CartRepository
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartItemAsync(Guid customerId, long productId, string variantSlug);
        Task<List<Cart>> GetAllCartItemsAsync(Guid customerId);
        Task AddOrUpdateCartItemAsync(Cart cartItem);
        Task RemoveCartItemsAsync(IEnumerable<Cart> cartItems);
        Task<int> GetCurrentQuantityAsync(Guid customerId, long productId, string variantSlug);
        Task<Cart?> GetCartItemByIdAsync(long cartId, Guid customerId);
    }

    public class CartRepository : ICartRepository
    {
        private readonly SQLServerDbContext _context;

        public CartRepository(SQLServerDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Cart?> GetCartItemAsync(Guid customerId, long productId, string variantSlug)
        {
            return await _context.Carts
                .FirstOrDefaultAsync(c =>
                    c.CustomerId == customerId &&
                    c.ProductId == productId &&
                    c.VariantSlug == variantSlug);
        }

        public async Task<List<Cart>> GetAllCartItemsAsync(Guid customerId)
        {
            return await _context.Carts
                .Where(c => c.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task AddOrUpdateCartItemAsync(Cart cartItem)
        {
            var existing = await GetCartItemAsync(cartItem.CustomerId, cartItem.ProductId, cartItem.VariantSlug);
            if (existing == null)
            {
                await _context.Carts.AddAsync(cartItem);
            }
            else
            {
                existing.Quantity = cartItem.Quantity;
                _context.Carts.Update(existing);
            }
            await _context.SaveChangesAsync();
        }

        public async Task RemoveCartItemsAsync(IEnumerable<Cart> cartItems)
        {
            _context.Carts.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCurrentQuantityAsync(Guid customerId, long productId, string variantSlug)
        {
            var item = await GetCartItemAsync(customerId, productId, variantSlug);
            return item?.Quantity ?? 0;
        }
        public async Task<Cart?> GetCartItemByIdAsync(long cartId, Guid customerId)
        {
            return await _context.Carts
                .FirstOrDefaultAsync(c => c.Id == cartId && c.CustomerId == customerId);
        }
    }
}