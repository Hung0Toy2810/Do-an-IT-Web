// Services/RedisProductViewService.cs
using StackExchange.Redis;

namespace Backend.Services
{
    public interface IRedisProductViewService
    {
        Task TrackViewAsync(long productId);
        Task<List<long>> GetTop30ViewedTodayAsync(); // Cache 15 phút
    }

    public class RedisProductViewService : IRedisProductViewService
    {
        private readonly IDatabase _db;
        private readonly ConnectionMultiplexer _redis;

        // Key thay đổi theo ngày (chỉ tính trong 24h)
        private string TodayKey => $"product:views:daily:{DateTime.UtcNow:yyyy-MM-dd}";
        private string CacheKey => $"cache:top30:daily:{DateTime.UtcNow:yyyy-MM-dd}";

        public RedisProductViewService(IConnectionMultiplexer redis)
        {
            _redis = (ConnectionMultiplexer)redis;
            _db = _redis.GetDatabase();
        }

        public async Task TrackViewAsync(long productId)
        {
            await _db.SortedSetIncrementAsync(TodayKey, productId, 1);
            await _db.KeyExpireAsync(TodayKey, TimeSpan.FromHours(48)); // giữ 2 ngày cho an toàn
        }

        public async Task<List<long>> GetTop30ViewedTodayAsync()
        {
            // 1. Kiểm tra cache trước (15 phút)
            var cached = await _db.StringGetAsync(CacheKey);
            if (cached.HasValue && !cached.IsNullOrEmpty)
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<long>>(cached!)!;
            }

            // 2. Lấy từ Redis Sorted Set
            var result = await _db.SortedSetRangeByRankAsync(TodayKey, 0, 29, Order.Descending);
            var productIds = result.Select(x => (long)x).ToList();

            // 3. Cache lại 15 phút
            if (productIds.Count > 0)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(productIds);
                await _db.StringSetAsync(CacheKey, json, TimeSpan.FromMinutes(15));
            }

            return productIds;
        }
    }
}