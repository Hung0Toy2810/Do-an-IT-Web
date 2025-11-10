using Backend.Model.Nosql.ViettelPost;
using Backend.Service.DbFactory;
using MongoDB.Driver;

namespace Backend.Repository.ViettelPost
{
    public class ViettelPostAddressRepository : IViettelPostAddressRepository
    {
        private readonly IMongoCollection<ProvinceDocument> _provinceCollection;
        private readonly IMongoCollection<DistrictDocument> _districtCollection;
        private readonly IMongoCollection<WardDocument> _wardCollection;

        public ViettelPostAddressRepository(IMongoDbContextFactory factory)
        {
            var context = factory.CreateContext();
            _provinceCollection = context.GetCollection<ProvinceDocument>("viettelpost_provinces");
            _districtCollection = context.GetCollection<DistrictDocument>("viettelpost_districts");
            _wardCollection = context.GetCollection<WardDocument>("viettelpost_wards");

            CreateIndexes();
        }

        private void CreateIndexes()
        {
            // === PROVINCE ===
            var provinceIndex = Builders<ProvinceDocument>.IndexKeys.Ascending(x => x.ProvinceId);
            _provinceCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProvinceDocument>(
                provinceIndex, new CreateIndexOptions { Unique = true }));

            var provinceTtlIndex = Builders<ProvinceDocument>.IndexKeys.Ascending(x => x.UpdatedAt);
            _provinceCollection.Indexes.CreateOneAsync(new CreateIndexModel<ProvinceDocument>(
                provinceTtlIndex, new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(5) }));

            // === DISTRICT ===
            var districtIndex = Builders<DistrictDocument>.IndexKeys
                .Ascending(x => x.DistrictId)
                .Ascending(x => x.ProvinceId);
            _districtCollection.Indexes.CreateOneAsync(new CreateIndexModel<DistrictDocument>(
                districtIndex, new CreateIndexOptions { Unique = true }));

            var districtTtlIndex = Builders<DistrictDocument>.IndexKeys.Ascending(x => x.UpdatedAt);
            _districtCollection.Indexes.CreateOneAsync(new CreateIndexModel<DistrictDocument>(
                districtTtlIndex, new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(5) }));

            // === WARD ===
            var wardIndex = Builders<WardDocument>.IndexKeys
                .Ascending(x => x.WardId)
                .Ascending(x => x.DistrictId);
            _wardCollection.Indexes.CreateOneAsync(new CreateIndexModel<WardDocument>(
                wardIndex, new CreateIndexOptions { Unique = true }));

            var wardTtlIndex = Builders<WardDocument>.IndexKeys.Ascending(x => x.UpdatedAt);
            _wardCollection.Indexes.CreateOneAsync(new CreateIndexModel<WardDocument>(
                wardTtlIndex, new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(5) }));
        }

        public async Task<List<ProvinceDocument>> GetAllProvincesAsync()
            => await _provinceCollection.Find(_ => true).ToListAsync();

        public async Task<ProvinceDocument?> GetProvinceByIdAsync(int provinceId)
            => await _provinceCollection.Find(x => x.ProvinceId == provinceId).FirstOrDefaultAsync();

        public async Task<List<DistrictDocument>> GetDistrictsByProvinceIdAsync(int provinceId)
            => await _districtCollection.Find(x => x.ProvinceId == provinceId).ToListAsync();

        public async Task<List<WardDocument>> GetWardsByDistrictIdAsync(int districtId)
            => await _wardCollection.Find(x => x.DistrictId == districtId).ToListAsync();

        public async Task UpsertProvincesAsync(List<ProvinceDocument> provinces)
        {
            var tasks = provinces.Select(p => 
                _provinceCollection.ReplaceOneAsync(
                    filter: x => x.ProvinceId == p.ProvinceId,
                    replacement: p,
                    options: new ReplaceOptions { IsUpsert = true }
                )
            );
            await Task.WhenAll(tasks);
        }

        public async Task UpsertDistrictsAsync(List<DistrictDocument> districts)
        {
            var tasks = districts.Select(d => _districtCollection.ReplaceOneAsync(
                x => x.DistrictId == d.DistrictId,
                d,
                new ReplaceOptions { IsUpsert = true }
            ));
            await Task.WhenAll(tasks);
        }

        public async Task UpsertWardsAsync(List<WardDocument> wards)
        {
            var tasks = wards.Select(w => _wardCollection.ReplaceOneAsync(
                x => x.WardId == w.WardId,
                w,
                new ReplaceOptions { IsUpsert = true }
            ));
            await Task.WhenAll(tasks);
        }
        // === XÓA TOÀN BỘ ===
        public Task DeleteAllProvincesAsync()
            => _provinceCollection.DeleteManyAsync(_ => true);

        public Task DeleteAllDistrictsAsync()
            => _districtCollection.DeleteManyAsync(_ => true);

        public Task DeleteAllWardsAsync()
            => _wardCollection.DeleteManyAsync(_ => true);
    }
}