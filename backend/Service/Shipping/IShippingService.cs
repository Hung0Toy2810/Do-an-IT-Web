using Backend.Model.Entity;
using Backend.Service.Checkout;

namespace Backend.Service.Shipping
{
    public interface IShippingService
    {
        Task<decimal> CalculateFeeAsync(ShippingAddress address, bool isCOD);
        Task<ShipmentResult> CreateShipmentAsync(long invoiceId, decimal codAmount, bool isCOD);

        // Method mới – dùng cho Background Simulation (có note + async + hoàn kho)
        Task SimulateStatusUpdate(long invoiceId, InvoiceStatus newStatus, string? note = null);
        
        // Method cũ – dùng cho CheckoutService (fire-and-forget, không hoàn kho)
        void SimulateStatusUpdateBackground(long invoiceId, InvoiceStatus status);
    }

    public class ShipmentResult
    {
        public bool Success { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Error { get; set; }
    }
}