// Repository/Product/ProductDocumentRepository.cs
using Backend.Model.Nosql;
using MongoDB.Driver;
using Backend.Service.DbFactory;

namespace Backend.Repository.Product
{
    public interface IProductDocumentRepository
    {
        Task<ProductDocument?> GetByIdAsync(long productId);
        Task<ProductDocument?> GetBySlugAsync(string slug);
        Task<ProductDocument?> GetByMongoIdAsync(string mongoId);
        Task<List<ProductDocument>> GetAllAsync();
        Task<List<ProductDocument>> GetByBrandAsync(string brand);
        Task<ProductDocument> CreateAsync(ProductDocument document);
        Task<bool> UpdateAsync(ProductDocument document);
        Task<bool> DeleteAsync(long productId);
        Task<bool> ExistsAsync(long productId);
        Task<bool> ExistsBySlugAsync(string slug);
        Task<ProductVariant?> GetVariantAsync(long productId, Dictionary<string, string> attributes);
        Task<bool> UpdateStockAsync(long productId, Dictionary<string, string> attributes, int stockChange);
        
        // Search methods
        Task<List<long>> SearchProductIdsByKeywordAsync(string keyword);
        Task<List<ProductDocument>> GetProductsByIdsAsync(List<long> productIds);
        Task<List<ProductDocument>> SearchProductsWithFiltersAsync(
            List<long> productIds, 
            decimal? minPrice, 
            decimal? maxPrice);
        
        // SubCategory methods
        Task<List<ProductDocument>> GetProductsByIdsWithBrandFilterAsync(
            List<long> productIds, 
            string? brand,
            decimal? minPrice,
            decimal? maxPrice);
        Task<List<string>> GetBrandsByProductIdsAsync(List<long> productIds);
    }
    
    public class ProductDocumentRepository : IProductDocumentRepository
    {
        private readonly IMongoCollection<ProductDocument> _collection;

        public ProductDocumentRepository(IMongoDbContextFactory factory)
        {
            var mongoContext = factory.CreateContext();
            _collection = mongoContext.GetCollection<ProductDocument>("products");
            
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            var indexModels = new[]
            {
                new CreateIndexModel<ProductDocument>(
                    Builders<ProductDocument>.IndexKeys.Ascending(x => x.Id),
                    new CreateIndexOptions { Unique = true }
                ),
                new CreateIndexModel<ProductDocument>(
                    Builders<ProductDocument>.IndexKeys.Ascending(x => x.Slug),
                    new CreateIndexOptions { Unique = true }
                ),
                new CreateIndexModel<ProductDocument>(
                    Builders<ProductDocument>.IndexKeys.Ascending(x => x.Brand)
                )
            };

            _collection.Indexes.CreateManyAsync(indexModels);
        }

        public async Task<ProductDocument?> GetByIdAsync(long productId)
        {
            return await _collection.Find(x => x.Id == productId).FirstOrDefaultAsync();
        }

        public async Task<ProductDocument?> GetBySlugAsync(string slug)
        {
            return await _collection.Find(x => x.Slug == slug).FirstOrDefaultAsync();
        }

        public async Task<ProductDocument?> GetByMongoIdAsync(string mongoId)
        {
            return await _collection.Find(x => x.MongoId == mongoId).FirstOrDefaultAsync();
        }

        public async Task<List<ProductDocument>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<List<ProductDocument>> GetByBrandAsync(string brand)
        {
            return await _collection.Find(x => x.Brand == brand).ToListAsync();
        }

        public async Task<ProductDocument> CreateAsync(ProductDocument document)
        {
            document.CreatedAt = DateTime.UtcNow;
            document.UpdatedAt = DateTime.UtcNow;
            await _collection.InsertOneAsync(document);
            return document;
        }

        public async Task<bool> UpdateAsync(ProductDocument document)
        {
            document.UpdatedAt = DateTime.UtcNow;
            var result = await _collection.ReplaceOneAsync(
                x => x.Id == document.Id,
                document
            );
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(long productId)
        {
            var result = await _collection.DeleteOneAsync(x => x.Id == productId);
            return result.DeletedCount > 0;
        }

        public async Task<bool> ExistsAsync(long productId)
        {
            return await _collection.Find(x => x.Id == productId).AnyAsync();
        }

        public async Task<bool> ExistsBySlugAsync(string slug)
        {
            return await _collection.Find(x => x.Slug == slug).AnyAsync();
        }

        public async Task<ProductVariant?> GetVariantAsync(long productId, Dictionary<string, string> attributes)
        {
            var product = await GetByIdAsync(productId);
            if (product == null) return null;

            return product.Variants.FirstOrDefault(v =>
                v.Attributes.Count == attributes.Count &&
                v.Attributes.All(a => attributes.ContainsKey(a.Key) && attributes[a.Key] == a.Value)
            );
        }

        public async Task<bool> UpdateStockAsync(long productId, Dictionary<string, string> attributes, int stockChange)
        {
            var product = await GetByIdAsync(productId);
            if (product == null) return false;

            var variantIndex = product.Variants.FindIndex(v =>
                v.Attributes.Count == attributes.Count &&
                v.Attributes.All(a => attributes.ContainsKey(a.Key) && attributes[a.Key] == a.Value)
            );

            if (variantIndex == -1) return false;

            var filter = Builders<ProductDocument>.Filter.Eq(x => x.Id, productId);
            var update = Builders<ProductDocument>.Update
                .Inc($"variants.{variantIndex}.stock", stockChange)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        // ============================================================
        // SEARCH METHODS
        // ============================================================

        // Tìm ProductId từ MongoDB theo từ khóa (search trong Name, Brand, Description)
        public async Task<List<long>> SearchProductIdsByKeywordAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<long>();

            var normalizedKeyword = keyword.Trim();

            // Tạo filter tìm kiếm trong Name, Brand, Description
            var filter = Builders<ProductDocument>.Filter.Or(
                Builders<ProductDocument>.Filter.Regex(x => x.Name, 
                    new MongoDB.Bson.BsonRegularExpression(normalizedKeyword, "i")),
                Builders<ProductDocument>.Filter.Regex(x => x.Brand, 
                    new MongoDB.Bson.BsonRegularExpression(normalizedKeyword, "i")),
                Builders<ProductDocument>.Filter.Regex(x => x.Description, 
                    new MongoDB.Bson.BsonRegularExpression(normalizedKeyword, "i"))
            );

            var productIds = await _collection
                .Find(filter)
                .Project(x => x.Id)
                .ToListAsync();

            return productIds;
        }

        // Lấy danh sách ProductDocument theo list ID
        public async Task<List<ProductDocument>> GetProductsByIdsAsync(List<long> productIds)
        {
            if (productIds == null || productIds.Count == 0)
                return new List<ProductDocument>();

            var filter = Builders<ProductDocument>.Filter.In(x => x.Id, productIds);
            return await _collection.Find(filter).ToListAsync();
        }

        // Tìm kiếm với filter giá
        public async Task<List<ProductDocument>> SearchProductsWithFiltersAsync(
            List<long> productIds,
            decimal? minPrice,
            decimal? maxPrice)
        {
            if (productIds == null || productIds.Count == 0)
                return new List<ProductDocument>();

            var filterBuilder = Builders<ProductDocument>.Filter;
            var filter = filterBuilder.In(x => x.Id, productIds);

            // Nếu có filter giá
            if (minPrice.HasValue || maxPrice.HasValue)
            {
                // Filter sản phẩm có ít nhất 1 variant thỏa mãn điều kiện giá
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

        // ============================================================
        // SUBCATEGORY METHODS
        // ============================================================

        // Lấy sản phẩm theo IDs với filter Brand và giá
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
                filterBuilder.In(x => x.Id, productIds)
            };

            // Filter theo Brand nếu có
            if (!string.IsNullOrWhiteSpace(brand))
            {
                filters.Add(filterBuilder.Eq(x => x.Brand, brand));
            }

            // Filter theo giá nếu có
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

        // Lấy danh sách Brand từ list ProductIds
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
    }
}