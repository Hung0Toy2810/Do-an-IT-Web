using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Backend.Repository.Product;
using Backend.Service.Product;
namespace Backend.Service
{
    public class StockCleanupService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<StockCleanupService> _logger;

        public StockCleanupService(IServiceProvider services, ILogger<StockCleanupService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var reservationRepo = scope.ServiceProvider.GetRequiredService<IStockReservationRepository>();
                    var stockService = scope.ServiceProvider.GetRequiredService<IProductStockService>();

                    var expired = await reservationRepo.GetExpiredReservationsAsync();
                    foreach (var res in expired)
                    {
                        await stockService.ReleaseStockAsync(res.OrderId);
                        _logger.LogInformation("Tự động giải phóng: Order {OrderId}, {Qty} {Variant}", 
                            res.OrderId, res.ReservedQuantity, res.VariantSlug);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi dọn stock hết hạn");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}