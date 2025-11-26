using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Backend.Repository.Product;
using Backend.Service.Product;
using Backend.Service.Checkout;
using Backend.Repository.InvoiceRepository;
namespace Backend.Service
{
    // Backend/Service/StockCleanupService.cs
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
            _logger.LogInformation("StockCleanupService khởi động");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var reservationRepo = scope.ServiceProvider.GetRequiredService<IStockReservationRepository>();
                    var invoiceRepo = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
                    var stockService = scope.ServiceProvider.GetRequiredService<IProductStockService>();
                    var context = scope.ServiceProvider.GetRequiredService<SQLServerDbContext>();

                    var expired = await reservationRepo.GetExpiredReservationsAsync();
                    var expiredByInvoice = expired.GroupBy(r => r.InvoiceId);

                    foreach (var group in expiredByInvoice)
                    {
                        var invoiceId = group.Key;
                        var invoice = await invoiceRepo.GetInvoiceByIdAsync(invoiceId);
                        
                        if (invoice != null && invoice.Status == (int)InvoiceStatus.Pending)
                        {
                            // HỦY ĐƠN
                            await invoiceRepo.UpdateInvoiceStatusAsync(invoice.Id, (int)InvoiceStatus.Cancelled);

                            context.InvoiceStatusHistories.Add(new InvoiceStatusHistory
                            {
                                InvoiceId = invoice.Id,
                                Status = "Cancelled",
                                Note = "Hết hạn thanh toán VNPay (20 phút) – tự động hủy",
                                CreatedAt = DateTime.UtcNow
                            });

                            _logger.LogWarning("TỰ ĐỘNG HỦY ĐƠN HẾT HẠN → Invoice {InvoiceId}", invoiceId);
                        }

                        // ✅ CHỈ RELEASE RESERVATION (cộng lại Variant.Stock)
                        // Không cần release batch vì chưa allocate!
                        await stockService.ReleaseStockAsync(invoiceId);

                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong StockCleanupService");
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }
}