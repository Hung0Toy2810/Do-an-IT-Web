// Services/BestSellerService.cs
using Backend.SQLDbContext;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class BestSellerService
    {
        private readonly SQLServerDbContext _context;
        public BestSellerService(SQLServerDbContext context) => _context = context;

        public Task<List<long>> GetTop30() => _context.InvoiceDetails
            .GroupBy(x => x.ProductId)
            .OrderByDescending(g => g.Sum(x => x.Quantity))
            .Select(g => g.Key)
            .Take(30)
            .ToListAsync();
    }
}