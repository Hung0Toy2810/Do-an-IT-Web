using MongoDB.Driver;
using System;

namespace Backend.NoSQLDbContext
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IMongoDatabase database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database), "MongoDB database cannot be null.");
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name), "Collection name cannot be null or empty.");
            }
            return _database.GetCollection<T>(name);
        }
    }
}