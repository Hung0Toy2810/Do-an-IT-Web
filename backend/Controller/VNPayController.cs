using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using Backend.Service.Payment;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/payment/vnpay")]
    public class VNPayController : ControllerBase
    {
        private readonly IVNPayService _vnpayService;
        private readonly ILogger<VNPayController> _logger;

        public VNPayController(IVNPayService vnpayService, ILogger<VNPayController> logger)
        {
            _vnpayService = vnpayService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo URL thanh toán VNPay
        /// POST /api/payment/vnpay/create
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                _logger.LogInformation($"Creating payment for Invoice {request.InvoiceId}, Amount {request.Amount}");
                
                string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                var result = await _vnpayService.CreatePaymentUrlAsync(request.InvoiceId, request.Amount, ip);
                
                if (!result.IsValid)
                {
                    _logger.LogError($"Validation failed: {result.ErrorMessage}");
                    return BadRequest(new 
                    { 
                        Success = false,
                        Error = result.ErrorMessage,
                        ErrorCode = "VALIDATION_FAILED"
                    });
                }
                
                _logger.LogInformation($"✅ VNPAY URL created successfully");
                
                return Ok(new 
                { 
                    Success = true,
                    PaymentUrl = result.PaymentUrl 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return BadRequest(new 
                { 
                    Success = false,
                    Error = ex.Message,
                    ErrorCode = "INTERNAL_ERROR"
                });
            }
        }

        /// <summary>
        /// URL người dùng quay lại sau khi thanh toán
        /// GET /api/payment/vnpay/return
        /// </summary>
        [HttpGet("return")]
        public IActionResult PaymentReturn()
        {
            try
            {
                _logger.LogInformation($"Begin VNPAY Return, URL={Request.QueryString}");
                
                if (Request.Query.Count == 0)
                {
                    return BadRequest("No query parameters");
                }

                var query = Request.Query;
                var nameValueCollection = new NameValueCollection();
                
                foreach (var item in query)
                {
                    if (!string.IsNullOrEmpty(item.Key) && item.Key.StartsWith("vnp_"))
                    {
                        nameValueCollection.Add(item.Key, item.Value.ToString());
                    }
                }

                var result = _vnpayService.ProcessPaymentReturn(nameValueCollection);

                // Hiển thị kết quả cho user
                if (result.IsSuccess)
                {
                    _logger.LogInformation($"Payment success, OrderId={result.OrderId}, VNPAY TranId={result.VnpayTranId}");
                    
                    return Ok(new
                    {
                        Success = true,
                        Message = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ",
                        OrderId = result.OrderId,
                        TransactionId = result.VnpayTranId,
                        Amount = result.Amount,
                        BankCode = result.BankCode
                    });
                }
                else
                {
                    _logger.LogWarning($"Payment failed, OrderId={result.OrderId}, Code={result.ResponseCode}");
                    
                    return Ok(new
                    {
                        Success = false,
                        Message = $"Có lỗi xảy ra trong quá trình xử lý. Mã lỗi: {result.ResponseCode}",
                        OrderId = result.OrderId,
                        ResponseCode = result.ResponseCode
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment return");
                return Ok(new { Success = false, Message = "Có lỗi xảy ra trong quá trình xử lý" });
            }
        }

        /// <summary>
        /// IPN - VNPay server callback để xác nhận giao dịch
        /// GET /api/payment/vnpay/ipn
        /// </summary>
        [HttpGet("ipn")]
        public IActionResult PaymentIPN()
        {
            try
            {
                _logger.LogInformation($"Begin VNPAY IPN, URL={Request.QueryString}");
                
                if (Request.Query.Count == 0)
                {
                    return Content("{\"RspCode\":\"99\",\"Message\":\"Input data required\"}", "application/json");
                }

                var query = Request.Query;
                var nameValueCollection = new NameValueCollection();
                
                foreach (var item in query)
                {
                    if (!string.IsNullOrEmpty(item.Key) && item.Key.StartsWith("vnp_"))
                    {
                        nameValueCollection.Add(item.Key, item.Value.ToString());
                    }
                }

                var result = _vnpayService.ProcessPaymentIPN(nameValueCollection);
                
                _logger.LogInformation($"IPN Result: {result.RspCode} - {result.Message}");
                
                return Content($"{{\"RspCode\":\"{result.RspCode}\",\"Message\":\"{result.Message}\"}}", "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing IPN");
                return Content("{\"RspCode\":\"99\",\"Message\":\"Unknown error\"}", "application/json");
            }
        }
    }

    public class CreatePaymentRequest
    {
        public long InvoiceId { get; set; }
        public decimal Amount { get; set; }
    }
}