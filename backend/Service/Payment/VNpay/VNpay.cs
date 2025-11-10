using Backend.Model.dto.Payment.VNpay;
using log4net;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;

namespace Backend.Service.Payment
{
    public interface IVNPayService
    {
        Task<(bool IsValid, string PaymentUrl, string ErrorMessage)> CreatePaymentUrlAsync(long invoiceId, decimal amount, string ipAddress);
        VNPayPaymentResponseDto ProcessPaymentReturn(NameValueCollection queryString);
        VNPayIPNResponseDto ProcessPaymentIPN(NameValueCollection queryString);
    }

    public class VNPayService : IVNPayService
    {
        private readonly IConfiguration _config;
        private readonly ILog _log;
        private readonly string _vnpUrl;
        private readonly string _vnpTmnCode;
        private readonly string _vnpHashSecret;
        private readonly string _vnpReturnUrl;

        public VNPayService(IConfiguration config)
        {
            _config = config;
            _log = LogManager.GetLogger(typeof(VNPayService));

            _vnpUrl = _config["VNPay:vnp_Url"]!;
            _vnpTmnCode = _config["VNPay:vnp_TmnCode"]!;
            _vnpHashSecret = _config["VNPay:vnp_HashSecret"]!;
            _vnpReturnUrl = _config["VNPay:vnp_Returnurl"]!;
        }

        /// <summary>
        /// Tạo payment URL với PRE-VALIDATION
        /// </summary>
        public async Task<(bool IsValid, string PaymentUrl, string ErrorMessage)> CreatePaymentUrlAsync(
            long invoiceId, decimal amount, string ipAddress)
        {
            // ✅ VALIDATION 1: Kiểm tra config
            if (string.IsNullOrEmpty(_vnpTmnCode) || _vnpTmnCode == "YOUR_TMN_CODE")
            {
                _log.Error("VNPay TmnCode chưa được cấu hình");
                return (false, string.Empty, "VNPay TmnCode chưa được cấu hình. Vui lòng kiểm tra appsettings.json");
            }

            if (string.IsNullOrEmpty(_vnpHashSecret) || _vnpHashSecret == "YOUR_HASH_SECRET")
            {
                _log.Error("VNPay HashSecret chưa được cấu hình");
                return (false, string.Empty, "VNPay HashSecret chưa được cấu hình. Vui lòng kiểm tra appsettings.json");
            }

            // ✅ VALIDATION 2: Kiểm tra ReturnUrl format
            if (!Uri.TryCreate(_vnpReturnUrl, UriKind.Absolute, out var uri))
            {
                _log.Error("ReturnUrl không đúng định dạng");
                return (false, string.Empty, "ReturnUrl không đúng định dạng URL");
            }

            if (uri.Scheme != "https" && uri.Scheme != "http")
            {
                _log.Error("ReturnUrl phải dùng http hoặc https");
                return (false, string.Empty, "ReturnUrl phải dùng http hoặc https");
            }

            // ✅ VALIDATION 3: Kiểm tra amount
            if (amount <= 0)
            {
                return (false, string.Empty, "Số tiền phải lớn hơn 0");
            }

            if (amount > 1000000000) // 1 tỷ VND
            {
                return (false, string.Empty, "Số tiền vượt quá giới hạn cho phép");
            }

            // ✅ Tạo payment URL
            var vnpay = new VnPayLibrary();
            var now = DateTime.UtcNow.AddHours(7);
            var expire = now.AddMinutes(15);

            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _vnpTmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", ipAddress ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang:{invoiceId}");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _vnpReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", invoiceId.ToString());
            vnpay.AddRequestData("vnp_ExpireDate", expire.ToString("yyyyMMddHHmmss"));

            // 🔍 DEBUG: In ra tất cả parameters
            Console.WriteLine("=== VNPay Request Parameters ===");
            Console.WriteLine($"vnp_Version: {VnPayLibrary.VERSION}");
            Console.WriteLine($"vnp_Command: pay");
            Console.WriteLine($"vnp_TmnCode: {_vnpTmnCode}");
            Console.WriteLine($"vnp_Amount: {((long)(amount * 100))}");
            Console.WriteLine($"vnp_CreateDate: {now:yyyyMMddHHmmss}");
            Console.WriteLine($"vnp_CurrCode: VND");
            Console.WriteLine($"vnp_IpAddr: {ipAddress ?? "127.0.0.1"}");
            Console.WriteLine($"vnp_Locale: vn");
            Console.WriteLine($"vnp_OrderInfo: Thanh toan don hang:{invoiceId}");
            Console.WriteLine($"vnp_OrderType: other");
            Console.WriteLine($"vnp_ReturnUrl: {_vnpReturnUrl}");
            Console.WriteLine($"vnp_TxnRef: {invoiceId}");
            Console.WriteLine($"vnp_ExpireDate: {expire:yyyyMMddHHmmss}");
            Console.WriteLine($"HashSecret: {_vnpHashSecret}");
            Console.WriteLine("================================");

            string paymentUrl = vnpay.CreateRequestUrl(_vnpUrl, _vnpHashSecret);
            
            _log.Info($"=== VNPay Payment URL Debug ===");
            _log.Info($"TmnCode: {_vnpTmnCode}");
            _log.Info($"Amount: {amount}");
            _log.Info($"InvoiceId: {invoiceId}");
            _log.Info($"ReturnUrl: {_vnpReturnUrl}");
            _log.Info($"Full URL: {paymentUrl}");
            _log.Info($"===============================");

            return (true, paymentUrl, string.Empty);
        }

        public VNPayPaymentResponseDto ProcessPaymentReturn(NameValueCollection queryString)
        {
            var vnpay = new VnPayLibrary();
            
            foreach (string key in queryString)
            {
                if (key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, queryString[key]!);
                }
            }

            string secureHash = queryString["vnp_SecureHash"]!;
            bool isValidSignature = vnpay.ValidateSignature(secureHash, _vnpHashSecret);

            if (!isValidSignature)
            {
                _log.Warn("Invalid signature in return URL");
                return new VNPayPaymentResponseDto
                {
                    ResponseCode = "97",
                    Message = "Invalid signature"
                };
            }

            long orderId = long.Parse(vnpay.GetResponseData("vnp_TxnRef"));
            long vnpayTranId = long.Parse(vnpay.GetResponseData("vnp_TransactionNo"));
            string responseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string transactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            decimal amount = decimal.Parse(vnpay.GetResponseData("vnp_Amount")) / 100;
            string bankCode = vnpay.GetResponseData("vnp_BankCode") ?? "";

            var result = new VNPayPaymentResponseDto
            {
                OrderId = orderId,
                VnpayTranId = vnpayTranId,
                ResponseCode = responseCode,
                TransactionStatus = transactionStatus,
                Amount = amount,
                BankCode = bankCode
            };

            if (result.IsSuccess)
            {
                result.Message = "Giao dịch thành công";
                _log.Info($"Payment success: OrderId={orderId}");
            }
            else
            {
                result.Message = $"Giao dịch thất bại. Mã lỗi: {responseCode}";
            }

            return result;
        }

        public VNPayIPNResponseDto ProcessPaymentIPN(NameValueCollection queryString)
        {
            var vnpay = new VnPayLibrary();
            
            foreach (string key in queryString)
            {
                if (key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, queryString[key]!);
                }
            }

            string secureHash = queryString["vnp_SecureHash"]!;
            bool isValidSignature = vnpay.ValidateSignature(secureHash, _vnpHashSecret);

            if (!isValidSignature)
            {
                _log.Error("Invalid signature in IPN");
                return new VNPayIPNResponseDto
                {
                    RspCode = "97",
                    Message = "Invalid signature"
                };
            }

            long orderId = long.Parse(vnpay.GetResponseData("vnp_TxnRef"));
            long vnpayTranId = long.Parse(vnpay.GetResponseData("vnp_TransactionNo"));
            decimal amount = decimal.Parse(vnpay.GetResponseData("vnp_Amount")) / 100;
            string responseCode = vnpay.GetResponseData("vnp_ResponseCode");
            string transactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");

            if (responseCode == "00" && transactionStatus == "00")
            {
                _log.Info($"✅ Payment success: OrderId={orderId}, Amount={amount:N0} VND, TranId={vnpayTranId}");
                Console.WriteLine($"✅ Thanh toán thành công với invoiceID: {orderId} với số tiền {amount:N0} VND");

                return new VNPayIPNResponseDto
                {
                    RspCode = "00",
                    Message = "Confirm Success"
                };
            }
            else
            {
                _log.Warn($"❌ Payment failed: OrderId={orderId}, Code={responseCode}");
                Console.WriteLine($"❌ Thanh toán thất bại. InvoiceID: {orderId}, Mã lỗi: {responseCode}");

                return new VNPayIPNResponseDto
                {
                    RspCode = "00",
                    Message = "Confirm Success"
                };
            }
        }
    }
}