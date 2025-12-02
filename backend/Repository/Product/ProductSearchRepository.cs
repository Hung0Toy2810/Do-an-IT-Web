using Backend.Model.Nosql;
using MongoDB.Driver;
using Backend.Service.DbFactory;

namespace Backend.Repository.Product
{
    public interface IProductSearchRepository
    {
        Task<List<long>> SearchProductIdsByKeywordAsync(string keyword);
        Task<List<ProductDocument>> GetProductsByIdsAsync(List<long> productIds);
        Task<List<ProductDocument>> SearchProductsWithFiltersAsync(
            List<long> productIds, 
            decimal? minPrice, 
            decimal? maxPrice);
        Task<List<ProductDocument>> GetProductsByIdsWithBrandFilterAsync(
            List<long> productIds, 
            string? brand,
            decimal? minPrice,
            decimal? maxPrice);
        Task<List<string>> GetBrandsByProductIdsAsync(List<long> productIds);
        Task<List<ProductDocument>> GetProductsWithAdvancedFiltersAsync(
            List<long> productIds,
            List<string>? brands,
            decimal? minPrice,
            decimal? maxPrice,
            bool? inStock,
            bool? onSale);
        // Trong IProductSearchRepository
        Task<List<long>> SearchAllProductIdsByKeywordAsync(string keyword);
        Task<List<ProductDocument>> SearchAllProductsWithFiltersAsync(
            List<long> productIds,
            string? brand,
            decimal? minPrice,
            decimal? maxPrice);
    }

    public class ProductSearchRepository : IProductSearchRepository
    {
        private readonly IMongoCollection<ProductDocument> _collection;

        public ProductSearchRepository(IMongoDbContextFactory factory)
        {
            var mongoContext = factory.CreateContext();
            _collection = mongoContext.GetCollection<ProductDocument>("products");
        }

        // 🔥 SỬ DỤNG TEXT SEARCH (improved)
        public async Task<List<long>> SearchProductIdsByKeywordAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<long>();

            var normalizedKeyword = keyword.Trim();

            var filter = Builders<ProductDocument>.Filter.And(
                Builders<ProductDocument>.Filter.Text(normalizedKeyword),
                Builders<ProductDocument>.Filter.Eq(x => x.IsDiscontinued, false)
            );

            var productIds = await _collection
                .Find(filter)
                .Project(x => x.Id)
                .Limit(1000)
                .ToListAsync();

            return productIds;
        }
        public async Task<List<ProductDocument>> GetProductsByIdsAsync(List<long> productIds)
        {
            if (productIds == null || productIds.Count == 0)
                return new List<ProductDocument>();

            var filter = Builders<ProductDocument>.Filter.In(x => x.Id, productIds);
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<ProductDocument>> SearchProductsWithFiltersAsync(
            List<long> productIds,
            decimal? minPrice,
            decimal? maxPrice)
        {
            if (productIds == null || productIds.Count == 0)
                return new List<ProductDocument>();

            var filterBuilder = Builders<ProductDocument>.Filter;
            
            var filter = filterBuilder.And(
                filterBuilder.In(x => x.Id, productIds),
                filterBuilder.Eq(x => x.IsDiscontinued, false)
            );

            if (minPrice.HasValue || maxPrice.HasValue)
            {
                filter = filterBuilder.And(
                    filter,
                    filterBuilder.ElemMatch(x => x.Variants, variant =>
                        (!minPrice.HasValue || variant.DiscountedPrice >= minPrice.Value) &&
                        (!maxPrice.HasValue || variant.DiscountedPrice <= maxPrice.Value)
                    )
                );
            }

            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<ProductDocument>> GetProductsByIdsWithBrandFilterAsync(
            List<long> productIds,
            string? brand,
            decimal? minPrice,
            decimal? maxPrice)
        {
            if (productIds == null || productIds.Count == 0)
                return new List<ProductDocument>();

            var filterBuilder = Builders<ProductDocument>.Filter;
            var filters = new List<FilterDefinition<ProductDocument>>
            {
                filterBuilder.In(x => x.Id, productIds),
                filterBuilder.Eq(x => x.IsDiscontinued, false)
            };

            if (!string.IsNullOrWhiteSpace(brand))
            {
                filters.Add(filterBuilder.Eq(x => x.Brand, brand));
            }

            if (minPrice.HasValue || maxPrice.HasValue)
            {
                filters.Add(
                    filterBuilder.ElemMatch(x => x.Variants, variant =>
                        (!minPrice.HasValue || variant.DiscountedPrice >= minPrice.Value) &&
                        (!maxPrice.HasValue || variant.DiscountedPrice <= maxPrice.Value)
                    )
                );
            }

            var finalFilter = filterBuilder.And(filters);
            return await _collection.Find(finalFilter).ToListAsync();
        }

        public async Task<List<string>> GetBrandsByProductIdsAsync(List<long> productIds)
        {
            if (productIds == null || productIds.Count == 0)
                return new List<string>();

            var filter = Builders<ProductDocument>.Filter.In(x => x.Id, productIds);
            
            var brands = await _collection
                .Find(filter)
                .Project(x => x.Brand)
                .ToListAsync();

            return brands.Distinct().OrderBy(b => b).ToList();
        }

        public async Task<List<ProductDocument>> GetProductsWithAdvancedFiltersAsync(
            List<long> productIds,
            List<string>? brands,
            decimal? minPrice,
            decimal? maxPrice,
            bool? inStock,
            bool? onSale)
        {
            if (productIds == null || productIds.Count == 0)
                return new List<ProductDocument>();

            var filterBuilder = Builders<ProductDocument>.Filter;
            var filters = new List<FilterDefinition<ProductDocument>>
            {
                filterBuilder.In(x => x.Id, productIds),
                filterBuilder.Eq(x => x.IsDiscontinued, false)
            };

            if (brands != null && brands.Count > 0)
            {
                filters.Add(filterBuilder.In(x => x.Brand, brands));
            }

            if (minPrice.HasValue || maxPrice.HasValue)
            {
                filters.Add(
                    filterBuilder.ElemMatch(x => x.Variants, variant =>
                        (!minPrice.HasValue || variant.DiscountedPrice >= minPrice.Value) &&
                        (!maxPrice.HasValue || variant.DiscountedPrice <= maxPrice.Value)
                    )
                );
            }

            if (inStock.HasValue && inStock.Value)
            {
                filters.Add(
                    filterBuilder.ElemMatch(x => x.Variants, variant => variant.Stock > 0)
                );
            }

            if (onSale.HasValue && onSale.Value)
            {
                filters.Add(
                    filterBuilder.ElemMatch(x => x.Variants,
                        variant => variant.DiscountedPrice < variant.OriginalPrice)
                );
            }

            var finalFilter = filterBuilder.And(filters);
            return await _collection.Find(finalFilter).ToListAsync();
        }
        
        // Trong ProductSearchRepository.cs

        public async Task<List<long>> SearchAllProductIdsByKeywordAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<long>();

            var normalizedKeyword = keyword.Trim();

            // Không lọc IsDiscontinued
            var filter = Builders<ProductDocument>.Filter.Text(normalizedKeyword);

            var productIds = await _collection
                .Find(filter)
                .Project(x => x.Id)
                .Limit(1000)
                .ToListAsync();

            return productIds;
        }

        public async Task<List<ProductDocument>> SearchAllProductsWithFiltersAsync(
            List<long> productIds,
            string? brand,
            decimal? minPrice,
            decimal? maxPrice)
        {
            if (productIds == null || productIds.Count == 0)
                return new List<ProductDocument>();

            var filterBuilder = Builders<ProductDocument>.Filter;
            var filters = new List<FilterDefinition<ProductDocument>>
            {
                filterBuilder.In(x => x.Id, productIds)
                // ✅ BỎ: filterBuilder.Eq(x => x.IsDiscontinued, false)
            };

            if (!string.IsNullOrWhiteSpace(brand))
            {
                filters.Add(filterBuilder.Eq(x => x.Brand, brand));
            }

            if (minPrice.HasValue || maxPrice.HasValue)
            {
                filters.Add(
                    filterBuilder.ElemMatch(x => x.Variants, variant =>
                        (!minPrice.HasValue || variant.DiscountedPrice >= minPrice.Value) &&
                        (!maxPrice.HasValue || variant.DiscountedPrice <= maxPrice.Value)
                    )
                );
            }

            var finalFilter = filterBuilder.And(filters);
            return await _collection.Find(finalFilter).ToListAsync();
        }
    }
}