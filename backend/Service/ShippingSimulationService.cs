using Backend.Model.Entity;
using Backend.Repository.InvoiceRepository;
using Backend.Service.Shipping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Backend.Service.Checkout;

namespace Backend.Services
{
    public class ShippingSimulationService : BackgroundService
    {
        private readonly ILogger<ShippingSimulationService> _logger;
        private readonly IServiceProvider _services;

        public ShippingSimulationService(IServiceProvider services, ILogger<ShippingSimulationService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ShippingSimulationService khởi động – theo dõi đơn Paid...");

            while (!stoppingToken.IsCancellationRequested)
            {
                // 🟢 Thời gian quét: 5 phút
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

                using var scope = _services.CreateScope();
                var invoiceRepo = scope.ServiceProvider.GetRequiredService<IInvoiceRepository>();
                var shippingService = scope.ServiceProvider.GetRequiredService<IShippingService>();

                var paidInvoices = await invoiceRepo.GetInvoicesByStatusAsync((int)InvoiceStatus.Paid);

                foreach (var invoice in paidInvoices)
                {
                    if (invoice.Status != (int)InvoiceStatus.Paid) continue;

                    // Chuyển trạng thái → Shipped
                    await shippingService.SimulateStatusUpdate(invoice.Id, InvoiceStatus.Shipped);
                    _logger.LogInformation("Đơn {Id} → Shipped", invoice.Id);

                    // Chờ ngẫu nhiên 20–60 giây
                    await Task.Delay(TimeSpan.FromSeconds(new Random().Next(20, 61)), stoppingToken);

                    // 88% Delivered, 12% Cancelled
                    if (new Random().NextDouble() < 0.88)
                    {
                        await shippingService.SimulateStatusUpdate(invoice.Id, InvoiceStatus.Delivered);
                        _logger.LogInformation("Đơn {Id} → Delivered ✅", invoice.Id);
                    }
                    else
                    {
                        await shippingService.SimulateStatusUpdate(
                            invoice.Id,
                            InvoiceStatus.Cancelled,
                            "Giao thất bại (mô phỏng)"
                        );

                        _logger.LogWarning("Đơn {Id} → Cancelled ❌", invoice.Id);
                    }
                }
            }
        }
    }
}
