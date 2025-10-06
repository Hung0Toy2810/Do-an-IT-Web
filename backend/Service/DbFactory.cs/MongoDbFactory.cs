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
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            _connectionString = configuration["MongoDb:ConnectionString"]
                ?? throw new InvalidOperationException("MongoDB connection string is missing in appsettings.json");

            _databaseName = configuration["MongoDb:Database"]
                ?? throw new InvalidOperationException("MongoDB database name is missing in appsettings.json");


            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("MongoDB connection string is not configured.");
            if (string.IsNullOrWhiteSpace(_databaseName))
                throw new InvalidOperationException("MongoDB database name is not configured.");
        }
        public MongoDbContext CreateContext()
        {
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase(_databaseName);
            return new MongoDbContext(database);
        }
    }
}