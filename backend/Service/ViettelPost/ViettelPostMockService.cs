using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using Backend.Model.Nosql.ViettelPost;

namespace Backend.Service.ViettelPost
{
    public interface IViettelPostMockService
    {
        Task<ViettelPostOrderResponse> CreateOrderAsync(ViettelPostOrderRequest request);
    }

    public class ViettelPostMockService : IViettelPostMockService
    {
        private readonly ILogger<ViettelPostMockService> _logger;
        private static int _counter = 1486360; // Bắt đầu từ số lớn để giống thật

        public ViettelPostMockService(ILogger<ViettelPostMockService> logger)
        {
            _logger = logger;
        }

        public Task<ViettelPostOrderResponse> CreateOrderAsync(ViettelPostOrderRequest request)
        {
            // TẠO ORDER_NUMBER GIẢ THEO ĐỊNH DẠNG VIETTELPOST
            var fakeOrderNumber = $"1023{_counter++:D8}";

            // TÍNH GIẢ LẬP PHÍ (dựa vào trọng lượng)
            int exchangeWeight = request.ProductWeight;
            if (request.ListItem?.Any() == true)
            {
                exchangeWeight = request.ListItem.Sum(x => x.ProductWeight * x.ProductQuantity);
            }

            int moneyTotalFee = exchangeWeight switch
            {
                <= 1000 => 25000,
                <= 3000 => 35000,
                <= 5000 => 45000,
                _ => 55000
            };

            var response = new ViettelPostOrderResponse
            {
                Status = 200,
                Error = false,
                Message = "OK",
                Data = new ViettelPostOrderData
                {
                    OrderNumber = fakeOrderNumber,
                    MoneyCollection = request.MoneyCollection,
                    ExchangeWeight = exchangeWeight,
                    MoneyTotal = moneyTotalFee + 10000,
                    MoneyTotalFee = moneyTotalFee,
                    MoneyFee = moneyTotalFee - 10000,
                    MoneyCollectionFee = 0,
                    MoneyOtherFee = 0,
                    MoneyVas = null,
                    MoneyVat = 10000,
                    KpiHt = 12
                }
            };

            _logger.LogInformation("[MOCK] Tạo đơn giả thành công: {OrderNumber} | Khối lượng: {Weight}g | Phí: {Fee:N0}đ", 
                fakeOrderNumber, exchangeWeight, moneyTotalFee);

            return Task.FromResult(response);
        }
    }
}