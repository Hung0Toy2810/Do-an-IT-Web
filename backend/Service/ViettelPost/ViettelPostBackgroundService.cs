using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Backend.Service.ViettelPost
{
    public class ViettelPostBackgroundService : BackgroundService
    {
        private readonly ILogger<ViettelPostBackgroundService> _logger;
        private readonly ViettelPostTokenService _tokenService;

        public ViettelPostBackgroundService(
            ILogger<ViettelPostBackgroundService> logger,
            ViettelPostTokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Viettel Post Background Service khởi động. Đảm bảo token luôn hợp lệ.");

            // Lần đầu login ngay
            await _tokenService.GetValidTokenAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Kiểm tra mỗi 1 phút
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    await _tokenService.GetValidTokenAsync(); // Tự động refresh nếu cần
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong Viettel Post Background Service");
                }
            }

            _logger.LogInformation("Viettel Post Background Service dừng.");
        }
    }
}