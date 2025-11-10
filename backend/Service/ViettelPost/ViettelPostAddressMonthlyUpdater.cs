using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Backend.Model.Nosql.ViettelPost;
using Backend.Repository.ViettelPost;
namespace Backend.Service.ViettelPost
{
    public class ViettelPostAddressMonthlyUpdater : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _config;
        private readonly ILogger<ViettelPostAddressMonthlyUpdater> _logger;

        public ViettelPostAddressMonthlyUpdater(
            IServiceProvider serviceProvider,
            IConfiguration config,
            ILogger<ViettelPostAddressMonthlyUpdater> logger)
        {
            _serviceProvider = serviceProvider;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // TẮT QUA CONFIG
            var enabled = _config.GetValue<bool>("ViettelPost:MonthlyAddressUpdate:Enabled", false);
            if (!enabled)
            {
                _logger.LogInformation("[ViettelPost] Monthly updater đã bị TẮT trong config.");
                return;
            }

            var intervalDays = _config.GetValue<int>("ViettelPost:MonthlyAddressUpdate:IntervalDays", 30);

            _logger.LogInformation("[ViettelPost] Monthly updater khởi động. Cập nhật mỗi {Days} ngày.", intervalDays);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateFullAddressAsync(stoppingToken);
                    _logger.LogInformation("[ViettelPost] Hoàn tất cập nhật. Ngủ {Days} ngày...", intervalDays);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[ViettelPost] Lỗi khi cập nhật địa chỉ hàng tháng");
                }

                await Task.Delay(TimeSpan.FromDays(intervalDays), stoppingToken);
            }
        }

        private async Task UpdateFullAddressAsync(CancellationToken stoppingToken)
        {
            _logger.LogWarning("[ViettelPost] BẮT ĐẦU CẬP NHẬT TOÀN BỘ ĐỊA CHỈ (XÓA CŨ → TẢI MỚI)...");

            using var scope = _serviceProvider.CreateScope();
            var addressService = scope.ServiceProvider.GetRequiredService<IViettelPostAddressService>();
            var mongoRepo = scope.ServiceProvider.GetRequiredService<IViettelPostAddressRepository>();

            // XÓA DỮ LIỆU CŨ
            await mongoRepo.DeleteAllProvincesAsync();
            await mongoRepo.DeleteAllDistrictsAsync();
            await mongoRepo.DeleteAllWardsAsync();
            _logger.LogInformation("[ViettelPost] Đã xóa toàn bộ dữ liệu cũ.");

            // 2. TẢI LẠI TỪ API
            var provinces = await addressService.GetAllProvincesAsync();
            _logger.LogInformation("[ViettelPost] Tải lại {Count} tỉnh.", provinces.Count);

            int totalDistricts = 0;
            int totalWards = 0;

            foreach (var province in provinces)
            {
                if (stoppingToken.IsCancellationRequested) break;

                var districts = await addressService.GetDistrictsByProvinceIdAsync(province.ProvinceId);
                totalDistricts += districts.Count;

                foreach (var district in districts)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var wards = await addressService.GetWardsByDistrictIdAsync(district.DistrictId);
                    totalWards += wards.Count;

                    await Task.Delay(50, stoppingToken);
                }

                await Task.Delay(100, stoppingToken);
            }

            _logger.LogWarning("[ViettelPost] HOÀN TẤT CẬP NHẬT! Tổng: {Provinces} tỉnh, {Districts} quận/huyện, {Wards} xã/phường",
                provinces.Count, totalDistricts, totalWards);
        }
    }

}