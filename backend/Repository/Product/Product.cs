// Repository/IProductDocumentRepository.cs
using Backend.Model.Nosql;
using MongoDB.Driver;

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
        Task<bool> ExistsBySlugAsync(string slug); // NEW: Check slug duplicate
        Task<ProductVariant?> GetVariantAsync(long productId, Dictionary<string, string> attributes);
        Task<bool> UpdateStockAsync(long productId, Dictionary<string, string> attributes, int stockChange);
    }
    
    public class ProductDocumentRepository : IProductDocumentRepository
    {
        private readonly IMongoCollection<ProductDocument> _collection;

        public ProductDocumentRepository(IMongoDbContextFactory factory)
        {
            var context = factory.CreateContext();
            _collection = context.GetCollection<ProductDocument>("products");
            
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

        // NEW METHOD
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
    }
}