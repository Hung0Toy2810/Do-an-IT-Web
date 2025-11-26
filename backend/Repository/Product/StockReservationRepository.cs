using Backend.Model.Nosql;
using Backend.Service.DbFactory;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Repository.Product
{
    public class StockReservation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? MongoId { get; set; }

        public long ProductId { get; set; }
        public string VariantSlug { get; set; } = string.Empty;
        public int ReservedQuantity { get; set; }
        
        public long InvoiceDetailId { get; set; }
        public long InvoiceId { get; set; }
        
        public DateTime ReservedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; } = "Reserved";
    }

    public interface IStockReservationRepository
    {
        Task<StockReservation> CreateAsync(StockReservation reservation);
        Task<StockReservation?> GetByDetailIdAsync(long invoiceDetailId); // 
        Task<List<StockReservation>> GetByInvoiceIdAsync(long invoiceId); // 
        Task<bool> UpdateStatusByDetailIdAsync(long invoiceDetailId, string status); // 
        Task<List<StockReservation>> GetExpiredReservationsAsync();
        Task<bool> DeleteByDetailIdAsync(long invoiceDetailId); // 
        Task<List<StockReservation>> GetActiveReservationsByProductIdAsync(long productId);
    }

    public class StockReservationRepository : IStockReservationRepository
    {
        private readonly IMongoCollection<StockReservation> _collection;

        public StockReservationRepository(IMongoDbContextFactory factory)
        {
            var mongoContext = factory.CreateContext();
            _collection = mongoContext.GetCollection<StockReservation>("stock_reservations");
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            var indexModels = new[]
            {
                //  Index theo InvoiceDetailId (UNIQUE)
                new CreateIndexModel<StockReservation>(
                    Builders<StockReservation>.IndexKeys.Ascending(x => x.InvoiceDetailId),
                    new CreateIndexOptions { Unique = true }
                ),
                //  Index theo InvoiceId (để query tất cả details của 1 invoice)
                new CreateIndexModel<StockReservation>(
                    Builders<StockReservation>.IndexKeys.Ascending(x => x.InvoiceId)
                ),
                // Index để tìm expired
                new CreateIndexModel<StockReservation>(
                    Builders<StockReservation>.IndexKeys.Ascending(x => x.ExpiresAt)
                ),
                // Index để tính available stock
                new CreateIndexModel<StockReservation>(
                    Builders<StockReservation>.IndexKeys
                        .Ascending(x => x.ProductId)
                        .Ascending(x => x.Status)
                        .Ascending(x => x.ExpiresAt)
                )
            };
            _collection.Indexes.CreateManyAsync(indexModels);
        }

        public async Task<StockReservation> CreateAsync(StockReservation reservation)
        {
            await _collection.InsertOneAsync(reservation);
            return reservation;
        }

        public async Task<StockReservation?> GetByDetailIdAsync(long invoiceDetailId)
        {
            return await _collection.Find(x => x.InvoiceDetailId == invoiceDetailId).FirstOrDefaultAsync();
        }

        public async Task<List<StockReservation>> GetByInvoiceIdAsync(long invoiceId)
        {
            return await _collection.Find(x => x.InvoiceId == invoiceId).ToListAsync();
        }

        public async Task<bool> UpdateStatusByDetailIdAsync(long invoiceDetailId, string status)
        {
            var update = Builders<StockReservation>.Update.Set(x => x.Status, status);
            var result = await _collection.UpdateOneAsync(x => x.InvoiceDetailId == invoiceDetailId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<StockReservation>> GetExpiredReservationsAsync()
        {
            return await _collection
                .Find(x => x.ExpiresAt <= DateTime.UtcNow && x.Status == "Reserved")
                .ToListAsync();
        }

        public async Task<bool> DeleteByDetailIdAsync(long invoiceDetailId)
        {
            var result = await _collection.DeleteOneAsync(x => x.InvoiceDetailId == invoiceDetailId);
            return result.DeletedCount > 0;
        }

        public async Task<List<StockReservation>> GetActiveReservationsByProductIdAsync(long productId)
        {
            var now = DateTime.UtcNow;
            return await _collection
                .Find(r => r.ProductId == productId && 
                        r.Status == "Reserved" && 
                        r.ExpiresAt > now)
                .ToListAsync();
        }
    }
}