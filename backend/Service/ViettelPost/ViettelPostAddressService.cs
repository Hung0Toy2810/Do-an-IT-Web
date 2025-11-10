using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Backend.Model.Nosql.ViettelPost;
using Backend.Repository.ViettelPost;
using Backend.Service.ViettelPost;
using MongoDB.Bson;

namespace Backend.Service.ViettelPost
{
    public class ViettelPostAddressService : IViettelPostAddressService
    {
        private readonly ViettelPostTokenService _tokenService;
        private readonly IViettelPostAddressRepository _mongoRepo;
        private readonly IConfiguration _config;
        private readonly ILogger<ViettelPostAddressService> _logger;
        private readonly HttpClient _httpClient;
        private readonly int _cacheDays;

        public ViettelPostAddressService(
            ViettelPostTokenService tokenService,
            IViettelPostAddressRepository mongoRepo,
            IConfiguration config,
            ILogger<ViettelPostAddressService> logger)
        {
            _tokenService = tokenService;
            _mongoRepo = mongoRepo;
            _config = config;
            _logger = logger;
            _httpClient = new HttpClient();
            _cacheDays = _config.GetValue<int>("ViettelPost:AddressCacheDays", 5);
        }

        public async Task<List<Province>> GetAllProvincesAsync() 
        {
            var provinces = await _mongoRepo.GetAllProvincesAsync();
            if (provinces.Any())
            {
                _logger.LogDebug("Lấy tỉnh từ MongoDB cache");
                Console.WriteLine($"[ViettelPost] Lấy {provinces.Count} tỉnh từ MongoDB cache");
                return provinces.Select(ToProvince).ToList();
            }

            var fetched = await FetchProvincesFromApiAsync();
            await _mongoRepo.UpsertProvincesAsync(fetched.Select(ToProvinceDocument).ToList());
            Console.WriteLine($"[ViettelPost] Lưu {fetched.Count} tỉnh vào MongoDB cache");
            return fetched;
        }

        public async Task<Province?> GetProvinceByIdAsync(int provinceId)
        {
            var doc = await _mongoRepo.GetProvinceByIdAsync(provinceId);
            return doc != null ? ToProvince(doc) : null;
        }

        public async Task<List<District>> GetDistrictsByProvinceIdAsync(int provinceId)
        {
            var districts = await _mongoRepo.GetDistrictsByProvinceIdAsync(provinceId);
            if (districts.Any())
            {
                Console.WriteLine($"[ViettelPost] Lấy {districts.Count} quận/huyện từ MongoDB (tỉnh {provinceId})");
                return districts.Select(ToDistrict).ToList();
            }

            var fetched = await FetchDistrictsFromApiAsync(provinceId);
            await _mongoRepo.UpsertDistrictsAsync(fetched.Select(ToDistrictDocument).ToList());
            Console.WriteLine($"[ViettelPost] Lưu {fetched.Count} quận/huyện vào MongoDB (tỉnh {provinceId})");
            return fetched;
        }

        public async Task<List<Ward>> GetWardsByDistrictIdAsync(int districtId)
        {
            var wards = await _mongoRepo.GetWardsByDistrictIdAsync(districtId);
            if (wards.Any())
            {
                Console.WriteLine($"[ViettelPost] Lấy {wards.Count} xã/phường từ MongoDB (huyện {districtId})");
                return wards.Select(ToWard).ToList();
            }

            var fetched = await FetchWardsFromApiAsync(districtId);
            await _mongoRepo.UpsertWardsAsync(fetched.Select(ToWardDocument).ToList());
            Console.WriteLine($"[ViettelPost] Lưu {fetched.Count} xã/phường vào MongoDB (huyện {districtId})");
            return fetched;
        }

        // === FETCH API ===
        private async Task<List<Province>> FetchProvincesFromApiAsync()
        {
            var token = await _tokenService.GetValidTokenAsync();
            var url = "https://partner.viettelpost.vn/v2/categories/listProvinceById?provinceId=-1";
            Console.WriteLine($"[ViettelPost] Gọi API lấy tất cả tỉnh/thành...");
            return await CallApiAsync<List<Province>>(url, token);
        }

        private async Task<List<District>> FetchDistrictsFromApiAsync(int provinceId)
        {
            var token = await _tokenService.GetValidTokenAsync();
            var url = $"https://partner.viettelpost.vn/v2/categories/listDistrict?provinceId={provinceId}";
            Console.WriteLine($"[ViettelPost] Gọi API lấy quận/huyện tỉnh {provinceId}...");
            return await CallApiAsync<List<District>>(url, token);
        }

        private async Task<List<Ward>> FetchWardsFromApiAsync(int districtId)
        {
            var token = await _tokenService.GetValidTokenAsync();
            var url = $"https://partner.viettelpost.vn/v2/categories/listWards?districtId={districtId}";
            Console.WriteLine($"[ViettelPost] Gọi API lấy xã/phường huyện {districtId}...");
            return await CallApiAsync<List<Ward>>(url, token);
        }

        private async Task<T> CallApiAsync<T>(string url, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Token", token);
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"API lỗi: {response.StatusCode} - {json}");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false // BẮT BUỘC = false
            };

            var result = JsonSerializer.Deserialize<ApiResponse<T>>(json, options)
                        ?? throw new InvalidOperationException($"API response null. JSON: {json}");

            if (result.Data == null)
                throw new InvalidOperationException($"API Data null. Status: {result.Status}, Message: {result.Message}, JSON: {json}");

            return result.Data;
        }

        // === MAPPER ===
        private Province ToProvince(ProvinceDocument d) => new()
        {
            ProvinceId = d.ProvinceId,
            ProvinceCode = d.ProvinceCode,
            ProvinceName = d.ProvinceName
        };

        private District ToDistrict(DistrictDocument d) => new()
        {
            DistrictId = d.DistrictId,
            DistrictValue = d.DistrictValue,
            DistrictName = d.DistrictName,
            ProvinceId = d.ProvinceId
        };

        private Ward ToWard(WardDocument d) => new()
        {
            WardId = d.WardId,
            WardName = d.WardName,
            DistrictId = d.DistrictId
        };

        // ĐÃ SỬA: THÊM Id = ObjectId.GenerateNewId()
        private ProvinceDocument ToProvinceDocument(Province p) => new()
        {
            Id = ObjectId.GenerateNewId(),
            ProvinceId = p.ProvinceId,
            ProvinceCode = p.ProvinceCode,
            ProvinceName = p.ProvinceName,
            UpdatedAt = DateTime.UtcNow
        };

        private DistrictDocument ToDistrictDocument(District d) => new()
        {
            Id = ObjectId.GenerateNewId(),
            DistrictId = d.DistrictId,
            DistrictValue = d.DistrictValue,
            DistrictName = d.DistrictName,
            ProvinceId = d.ProvinceId,
            UpdatedAt = DateTime.UtcNow
        };

        private WardDocument ToWardDocument(Ward w) => new()
        {
            Id = ObjectId.GenerateNewId(),
            WardId = w.WardId,
            WardName = w.WardName,
            DistrictId = w.DistrictId,
            UpdatedAt = DateTime.UtcNow
        };
    }
}