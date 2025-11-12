using Backend.Model.Nosql;
using Backend.Service.DbFactory;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Repository.Product
{
    public class StockReservation
    {
        [BsonId] // Bắt buộc, nói với driver đây là _id
        [BsonRepresentation(BsonType.ObjectId)]
        public string? MongoId { get; set; } // MongoId sẽ map với _id

        public long ProductId { get; set; }
        public string VariantSlug { get; set; } = string.Empty;
        public int ReservedQuantity { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public DateTime ReservedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; } = "Reserved";
    }

    public interface IStockReservationRepository
    {
        Task<StockReservation> CreateAsync(StockReservation reservation);
        Task<StockReservation?> GetByOrderIdAsync(string orderId);
        Task<bool> UpdateStatusAsync(string orderId, string status);
        Task<List<StockReservation>> GetExpiredReservationsAsync();
        Task<bool> DeleteAsync(string orderId);

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
                new CreateIndexModel<StockReservation>(
                    Builders<StockReservation>.IndexKeys.Ascending(x => x.OrderId),
                    new CreateIndexOptions { Unique = true }
                ),
                new CreateIndexModel<StockReservation>(
                    Builders<StockReservation>.IndexKeys.Ascending(x => x.ExpiresAt)
                )
            };
            _collection.Indexes.CreateManyAsync(indexModels);
        }

        public async Task<StockReservation> CreateAsync(StockReservation reservation)
        {
            await _collection.InsertOneAsync(reservation);
            return reservation;
        }

        public async Task<StockReservation?> GetByOrderIdAsync(string orderId)
        {
            return await _collection.Find(x => x.OrderId == orderId).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateStatusAsync(string orderId, string status)
        {
            var update = Builders<StockReservation>.Update.Set(x => x.Status, status);
            var result = await _collection.UpdateOneAsync(x => x.OrderId == orderId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<List<StockReservation>> GetExpiredReservationsAsync()
        {
            return await _collection
                .Find(x => x.ExpiresAt <= DateTime.UtcNow && x.Status == "Reserved")
                .ToListAsync();
        }

        public async Task<bool> DeleteAsync(string orderId)
        {
            var result = await _collection.DeleteOneAsync(x => x.OrderId == orderId);
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