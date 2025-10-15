using Backend.Model.Entity;
using Microsoft.EntityFrameworkCore;
using Backend.SQLDbContext;

namespace Backend.Repository.Product
{
    public interface IProductRepository
    {
        Task<List<long>> SearchProductIdsBySubCategoryAsync(string keyword);
        Task<List<long>> GetProductIdsBySubCategorySlugAsync(string subCategorySlug);
        Task<long?> GetSubCategoryIdBySlugAsync(string subCategorySlug);
        Task<List<long>> GetAllProductIdsAsync();


    }

    public class ProductRepository : IProductRepository
    {
        private readonly SQLServerDbContext _context;

        public ProductRepository(SQLServerDbContext context)
        {
            _context = context;
        }

        // Tìm ProductId từ SQL Server theo từ khóa (search trong Category, SubCategory)
        public async Task<List<long>> SearchProductIdsBySubCategoryAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<long>();

            // Chuẩn hóa keyword
            var normalizedKeyword = keyword.Trim().ToLower();

            // Tìm trong SubCategory name, Category name
            var productIds = await _context.Products
                .Include(p => p.SubCategory)
                    .ThenInclude(sc => sc.Category)
                .Where(p =>
                    EF.Functions.Like(p.SubCategory.Name.ToLower(), $"%{normalizedKeyword}%") ||
                    EF.Functions.Like(p.SubCategory.Category.Name.ToLower(), $"%{normalizedKeyword}%")
                )
                .Select(p => p.Id)
                .Distinct()
                .ToListAsync();

            return productIds;
        }
        // Lấy tất cả ProductId trong một SubCategory
        public async Task<List<long>> GetProductIdsBySubCategorySlugAsync(string subCategorySlug)
        {
            if (string.IsNullOrWhiteSpace(subCategorySlug))
                return new List<long>();

            var productIds = await _context.Products
                .Include(p => p.SubCategory)
                .Where(p => p.SubCategory.Slug == subCategorySlug)
                .Select(p => p.Id)
                .ToListAsync();

            return productIds;
        }

        // Lấy SubCategoryId theo slug
        public async Task<long?> GetSubCategoryIdBySlugAsync(string subCategorySlug)
        {
            var subCategory = await _context.SubCategories
                .FirstOrDefaultAsync(sc => sc.Slug == subCategorySlug);

            return subCategory?.Id;
        }

        public async Task<List<long>> GetAllProductIdsAsync()
        {
            return await _context.Products
                .Select(p => p.Id)
                .ToListAsync();
        }
    }
}