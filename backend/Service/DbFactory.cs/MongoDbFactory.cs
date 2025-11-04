using Backend.NoSQLDbContext;
using MongoDB.Driver;

namespace Backend.Service.DbFactory
{
    public interface IMongoDbContextFactory
    {
        MongoDbContext CreateContext();
    }

    public class MongoDbContextFactory : IMongoDbContextFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _connectionString;
        private readonly string _databaseName;

        public MongoDbContextFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider), "Đối tượng dịch vụ không được để trống.");

            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            _connectionString = configuration["MongoDb:ConnectionString"]
                ?? throw new InvalidOperationException("Không tìm thấy chuỗi kết nối MongoDB trong file appsettings.json.");

            _databaseName = configuration["MongoDb:Database"]
                ?? throw new InvalidOperationException("Không tìm thấy tên cơ sở dữ liệu MongoDB trong file appsettings.json.");

            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("Chuỗi kết nối MongoDB chưa được cấu hình hoặc bị bỏ trống.");

            if (string.IsNullOrWhiteSpace(_databaseName))
                throw new InvalidOperationException("Tên cơ sở dữ liệu MongoDB chưa được cấu hình hoặc bị bỏ trống.");
        }
        public MongoDbContext CreateContext()
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase(_databaseName);
            return new MongoDbContext(database);
        }
    }
}
