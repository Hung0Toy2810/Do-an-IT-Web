using Backend.Model.Nosql;
using MongoDB.Driver;
using Backend.Service.DbFactory;

namespace Backend.Repository.Product
{
    public interface IProductStockRepository
    {
        Task<ProductVariant?> GetVariantAsync(long productId, Dictionary<string, string> attributes);
        Task<bool> UpdateStockAsync(long productId, Dictionary<string, string> attributes, int stockChange);
        Task<int> BulkUpdateStockAsync(List<(long ProductId, string VariantSlug, int StockChange)> updates);
    }

    public class ProductStockRepository : IProductStockRepository
    {
        private readonly IMongoCollection<ProductDocument> _collection;

        public ProductStockRepository(IMongoDbContextFactory factory)
        {
            var mongoContext = factory.CreateContext();
            _collection = mongoContext.GetCollection<ProductDocument>("products");
        }

        public async Task<ProductVariant?> GetVariantAsync(long productId, Dictionary<string, string> attributes)
        {
            var filter = Builders<ProductDocument>.Filter.Eq(x => x.Id, productId);
            var product = await _collection.Find(filter).FirstOrDefaultAsync();
            
            if (product == null) return null;

            return product.Variants.FirstOrDefault(v =>
                v.Attributes.Count == attributes.Count &&
                v.Attributes.All(a => attributes.ContainsKey(a.Key) && attributes[a.Key] == a.Value)
            );
        }

        public async Task<bool> UpdateStockAsync(long productId, Dictionary<string, string> attributes, int stockChange)
        {
            var filter = Builders<ProductDocument>.Filter.Eq(x => x.Id, productId);
            var product = await _collection.Find(filter).FirstOrDefaultAsync();
            
            if (product == null) return false;

            var variantIndex = product.Variants.FindIndex(v =>
                v.Attributes.Count == attributes.Count &&
                v.Attributes.All(a => attributes.ContainsKey(a.Key) && attributes[a.Key] == a.Value)
            );

            if (variantIndex == -1) return false;

            var updateFilter = Builders<ProductDocument>.Filter.Eq(x => x.Id, productId);
            var update = Builders<ProductDocument>.Update
                .Inc($"variants.{variantIndex}.stock", stockChange)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(updateFilter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<int> BulkUpdateStockAsync(List<(long ProductId, string VariantSlug, int StockChange)> updates)
        {
            var bulkOps = new List<WriteModel<ProductDocument>>();

            foreach (var (productId, variantSlug, stockChange) in updates)
            {
                var filter = Builders<ProductDocument>.Filter.And(
                    Builders<ProductDocument>.Filter.Eq(x => x.Id, productId),
                    Builders<ProductDocument>.Filter.ElemMatch(
                        x => x.Variants,
                        v => v.Slug == variantSlug
                    )
                );

                var update = Builders<ProductDocument>.Update
                    .Inc("Variants.$.Stock", stockChange)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow);

                bulkOps.Add(new UpdateOneModel<ProductDocument>(filter, update));
            }

            if (bulkOps.Count == 0)
                return 0;

            var result = await _collection.BulkWriteAsync(bulkOps);
            return (int)result.ModifiedCount;
        }
    }
}