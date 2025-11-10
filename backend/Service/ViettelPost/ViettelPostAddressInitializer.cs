using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Backend.Service.ViettelPost
{
    public class ViettelPostAddressInitializer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ViettelPostAddressInitializer> _logger;

        public ViettelPostAddressInitializer(
            IServiceProvider serviceProvider,
            ILogger<ViettelPostAddressInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[ViettelPost] BẮT ĐẦU TẢI ĐẦY ĐỦ ĐỊA CHỈ VIỆT NAM...");

            using var scope = _serviceProvider.CreateScope();
            var addressService = scope.ServiceProvider.GetRequiredService<IViettelPostAddressService>();

            try
            {
                // 1. TẢI TỈNH
                var provinces = await addressService.GetAllProvincesAsync();
                _logger.LogInformation("[ViettelPost] Đã tải {Count} tỉnh/thành.", provinces.Count);

                int totalDistricts = 0;
                int totalWards = 0;

                // 2. DUYỆT TỪNG TỈNH → LẤY QUẬN/HUYỆN
                foreach (var province in provinces)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var districts = await addressService.GetDistrictsByProvinceIdAsync(province.ProvinceId);
                    totalDistricts += districts.Count;

                    _logger.LogInformation("[ViettelPost] Tỉnh {Name}: {Count} quận/huyện", province.ProvinceName, districts.Count);

                    // 3. DUYỆT TỪNG QUẬN → LẤY XÃ/PHƯỜNG
                    foreach (var district in districts)
                    {
                        if (stoppingToken.IsCancellationRequested) break;

                        var wards = await addressService.GetWardsByDistrictIdAsync(district.DistrictId);
                        totalWards += wards.Count;

                        _logger.LogInformation("  ├─ Huyện {Name}: {Count} xã/phường", district.DistrictName, wards.Count);

                        // Delay nhẹ để tránh quá tải API
                        await Task.Delay(50, stoppingToken);
                    }

                    // Delay giữa các tỉnh
                    await Task.Delay(100, stoppingToken);
                }

                _logger.LogInformation("[ViettelPost] HOÀN TẤT! Tổng: {Provinces} tỉnh, {Districts} quận/huyện, {Wards} xã/phường",
                    provinces.Count, totalDistricts, totalWards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ViettelPost] Lỗi khi tải dữ liệu địa chỉ");
            }
        }
    }
}