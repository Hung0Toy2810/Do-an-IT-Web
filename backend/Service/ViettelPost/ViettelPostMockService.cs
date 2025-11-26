// Backend/Service/ViettelPost/ViettelPostMockService.cs
using Backend.Model.Nosql.ViettelPost;
using Microsoft.Extensions.Logging;

namespace Backend.Service.ViettelPost
{
    public interface IViettelPostMockService
    {
        Task<ViettelPostOrderResponse> CreateOrderAsync(ViettelPostOrderRequest request);
    }

    public class ViettelPostMockService : IViettelPostMockService
    {
        private readonly ILogger<ViettelPostMockService> _logger;
        private static int _counter = 1486360;

        public ViettelPostMockService(ILogger<ViettelPostMockService> logger)
        {
            _logger = logger;
        }

        public Task<ViettelPostOrderResponse> CreateOrderAsync(ViettelPostOrderRequest request)
        {
            var orderNumber = $"1023{_counter++:D8}";

            int weight = request.ListItem?.Sum(x => x.ProductWeight * x.ProductQuantity) ?? request.ProductWeight;

            const int baseFee = 50000;
            const int vat = 2500;
            int totalFee = baseFee + vat;

            var response = new ViettelPostOrderResponse
            {
                Status = 200,
                Error = false,
                Message = "OK",
                Data = new ViettelPostOrderData
                {
                    OrderNumber = orderNumber,
                    MoneyCollection = request.MoneyCollection,
                    ExchangeWeight = weight,
                    MoneyTotal = totalFee,
                    MoneyTotalFee = baseFee,
                    MoneyFee = baseFee - 10000,
                    MoneyCollectionFee = request.OrderPayment == 3 ? (int)(request.MoneyCollection * 0.02m) : 0,
                    MoneyOtherFee = 0,
                    MoneyVas = null,
                    MoneyVat = vat,
                    KpiHt = 12
                }
            };

            _logger.LogInformation("[MOCK] Tạo đơn: {OrderNumber} | COD: {Cod} | Phí: {Fee:N0}đ", 
                orderNumber, request.MoneyCollection, totalFee);

            return Task.FromResult(response);
        }
    }
}