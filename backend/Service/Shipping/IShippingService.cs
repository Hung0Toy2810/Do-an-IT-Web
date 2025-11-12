// Backend/Service/Shipping/IShippingService.cs
namespace Backend.Service.Shipping
{
    public interface IShippingService
    {
        Task<decimal> CalculateFeeAsync(ShippingAddress address);
        Task<ShipmentResult> CreateShipmentAsync(long invoiceId, decimal codAmount);
    }

    public class ShipmentResult
    {
        public bool Success { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Error { get; set; }
    }
}