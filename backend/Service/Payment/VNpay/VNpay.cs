using System.Collections.Specialized;
using Backend.Model.dto.Payment.VNpay;
using log4net;

namespace Backend.Service.Payment
{
    public interface IVNPayService
    {

        /// Tạo URL thanh toán VNPay
        string CreatePaymentUrl(VNPayPaymentRequestDto request, string ipAddress);

        // Xử lý response trả về từ VNPay (Return URL)
        VNPayPaymentResponseDto ProcessPaymentReturn(NameValueCollection queryString);

        // Xử lý IPN callback từ VNPay
        VNPayIPNResponseDto ProcessPaymentIPN(NameValueCollection queryString);

        // Validate chữ ký từ VNPay
        bool ValidateSignature(NameValueCollection queryString, string secureHash);
    }

    public class VNPayService : IVNPayService
    {
        private readonly IConfiguration _configuration;
        private readonly ILog _log;
        private readonly string _vnpHashSecret;
        private readonly string _vnpUrl;
        private readonly string _vnpTmnCode;
        private readonly string _vnpReturnUrl;

        public VNPayService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _log = LogManager.GetLogger(typeof(VNPayService));
            
            // Load config với null checking
            _vnpHashSecret = _configuration["VNPay:vnp_HashSecret"] 
                ?? throw new InvalidOperationException("VNPay:vnp_HashSecret chưa được cấu hình");
            
            _vnpUrl = _configuration["VNPay:vnp_Url"] 
                ?? throw new InvalidOperationException("VNPay:vnp_Url chưa được cấu hình");
            
            _vnpTmnCode = _configuration["VNPay:vnp_TmnCode"] 
                ?? throw new InvalidOperationException("VNPay:vnp_TmnCode chưa được cấu hình");
            
            _vnpReturnUrl = _configuration["VNPay:vnp_Returnurl"] 
                ?? throw new InvalidOperationException("VNPay:vnp_Returnurl chưa được cấu hình");

            _log.Info("VNPayService initialized successfully");
        }

        public string CreatePaymentUrl(VNPayPaymentRequestDto request, string ipAddress)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request), "Yêu cầu thanh toán không được null");

                if (string.IsNullOrEmpty(ipAddress))
                    ipAddress = "127.0.0.1";

                var vnpay = new VnPayLibrary();

                // Add required parameters
                vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", _vnpTmnCode);
                vnpay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString());
                
                if (!string.IsNullOrEmpty(request.BankCode))
                {
                    vnpay.AddRequestData("vnp_BankCode", request.BankCode);
                }

                vnpay.AddRequestData("vnp_CreateDate", request.CreatedDate.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", ipAddress);
                vnpay.AddRequestData("vnp_Locale", string.IsNullOrEmpty(request.Locale) ? "vn" : request.Locale);
                vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + request.OrderId);
                vnpay.AddRequestData("vnp_OrderType", string.IsNullOrEmpty(request.OrderType) ? "other" : request.OrderType);
                vnpay.AddRequestData("vnp_ReturnUrl", _vnpReturnUrl);
                vnpay.AddRequestData("vnp_TxnRef", request.OrderId.ToString());

                // Add optional parameters
                if (!string.IsNullOrEmpty(request.ExpireDate))
                {
                    vnpay.AddRequestData("vnp_ExpireDate", request.ExpireDate);
                }

                // Billing information
                if (!string.IsNullOrEmpty(request.BillingMobile))
                    vnpay.AddRequestData("vnp_Bill_Mobile", request.BillingMobile);
                
                if (!string.IsNullOrEmpty(request.BillingEmail))
                    vnpay.AddRequestData("vnp_Bill_Email", request.BillingEmail);

                if (!string.IsNullOrEmpty(request.BillingFullName))
                {
                    var indexof = request.BillingFullName.IndexOf(' ');
                    if (indexof > 0)
                    {
                        vnpay.AddRequestData("vnp_Bill_FirstName", request.BillingFullName.Substring(0, indexof));
                        vnpay.AddRequestData("vnp_Bill_LastName", request.BillingFullName.Substring(indexof + 1));
                    }
                    else
                    {
                        vnpay.AddRequestData("vnp_Bill_FirstName", request.BillingFullName);
                        vnpay.AddRequestData("vnp_Bill_LastName", request.BillingFullName);
                    }
                }

                if (!string.IsNullOrEmpty(request.BillingAddress))
                    vnpay.AddRequestData("vnp_Bill_Address", request.BillingAddress);
                
                if (!string.IsNullOrEmpty(request.BillingCity))
                    vnpay.AddRequestData("vnp_Bill_City", request.BillingCity);
                
                if (!string.IsNullOrEmpty(request.BillingCountry))
                    vnpay.AddRequestData("vnp_Bill_Country", request.BillingCountry);

                vnpay.AddRequestData("vnp_Bill_State", "");

                // Invoice information
                if (!string.IsNullOrEmpty(request.InvoicePhone))
                    vnpay.AddRequestData("vnp_Inv_Phone", request.InvoicePhone);
                
                if (!string.IsNullOrEmpty(request.InvoiceEmail))
                    vnpay.AddRequestData("vnp_Inv_Email", request.InvoiceEmail);
                
                if (!string.IsNullOrEmpty(request.InvoiceCustomer))
                    vnpay.AddRequestData("vnp_Inv_Customer", request.InvoiceCustomer);
                
                if (!string.IsNullOrEmpty(request.InvoiceAddress))
                    vnpay.AddRequestData("vnp_Inv_Address", request.InvoiceAddress);
                
                if (!string.IsNullOrEmpty(request.InvoiceCompany))
                    vnpay.AddRequestData("vnp_Inv_Company", request.InvoiceCompany);
                
                if (!string.IsNullOrEmpty(request.InvoiceTaxCode))
                    vnpay.AddRequestData("vnp_Inv_Taxcode", request.InvoiceTaxCode);
                
                if (!string.IsNullOrEmpty(request.InvoiceType))
                    vnpay.AddRequestData("vnp_Inv_Type", request.InvoiceType);

                string paymentUrl = vnpay.CreateRequestUrl(_vnpUrl, _vnpHashSecret);
                _log.InfoFormat("VNPAY URL: {0}", paymentUrl);

                return paymentUrl;
            }
            catch (Exception ex)
            {
                _log.Error("Lỗi khi tạo URL thanh toán VNPay", ex);
                throw;
            }
        }

        public VNPayPaymentResponseDto ProcessPaymentReturn(NameValueCollection queryString)
        {
            try
            {
                if (queryString == null || queryString.Count == 0)
                    throw new ArgumentException("QueryString rỗng", nameof(queryString));

                _log.InfoFormat("Begin VNPAY Return, QueryString Count={0}", queryString.Count);

                var vnpay = new VnPayLibrary();
                
                foreach (string key in queryString)
                {
                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    {
                        string? value = queryString[key];
                        if (!string.IsNullOrEmpty(value))
                        {
                            vnpay.AddResponseData(key, value);
                        }
                    }
                }

                string txnRefStr = vnpay.GetResponseData("vnp_TxnRef");
                string transactionNoStr = vnpay.GetResponseData("vnp_TransactionNo");
                string amountStr = vnpay.GetResponseData("vnp_Amount");
                string vnpResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnpTransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                string? vnpSecureHash = queryString["vnp_SecureHash"];
                string? terminalId = queryString["vnp_TmnCode"];
                string? bankCode = queryString["vnp_BankCode"];

                // Parse với error handling
                if (!long.TryParse(txnRefStr, out long orderId))
                {
                    throw new FormatException("Định dạng vnp_TxnRef không hợp lệ");
                }

                if (!long.TryParse(transactionNoStr, out long vnpayTranId))
                {
                    throw new FormatException("Định dạng vnp_TransactionNo không hợp lệ");
                }

                if (!long.TryParse(amountStr, out long amountInCents))
                {
                    throw new FormatException("Định dạng vnp_Amount không hợp lệ");
                }

                decimal vnpAmount = amountInCents / 100m;

                var response = new VNPayPaymentResponseDto
                {
                    OrderId = orderId,
                    VnpayTranId = vnpayTranId,
                    ResponseCode = vnpResponseCode ?? string.Empty,
                    TransactionStatus = vnpTransactionStatus ?? string.Empty,
                    Amount = vnpAmount,
                    BankCode = bankCode ?? string.Empty,
                    TerminalId = terminalId ?? string.Empty
                };

                if (string.IsNullOrEmpty(vnpSecureHash))
                {
                    _log.Error("vnp_SecureHash bị thiếu");
                    response.Message = "Có lỗi xảy ra trong quá trình xử lý";
                    return response;
                }

                bool checkSignature = vnpay.ValidateSignature(vnpSecureHash, _vnpHashSecret);

                if (!checkSignature)
                {
                    _log.Error("Chữ ký từ VNPay không hợp lệ");
                    response.Message = "Có lỗi xảy ra trong quá trình xử lý";
                    return response;
                }

                if (response.IsSuccess)
                {
                    response.Message = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ";
                    _log.InfoFormat("Thanh toan thanh cong, OrderId={0}, VNPAY TranId={1}", orderId, vnpayTranId);
                }
                else
                {
                    response.Message = $"Có lỗi xảy ra trong quá trình xử lý. Mã lỗi: {vnpResponseCode}";
                    _log.InfoFormat("Thanh toan loi, OrderId={0}, VNPAY TranId={1}, ResponseCode={2}", 
                        orderId, vnpayTranId, vnpResponseCode);
                }

                return response;
            }
            catch (Exception ex)
            {
                _log.Error("Lỗi khi xử lý return từ VNPay", ex);
                throw;
            }
        }

        public VNPayIPNResponseDto ProcessPaymentIPN(NameValueCollection queryString)
        {
            try
            {
                if (queryString == null || queryString.Count == 0)
                {
                    return new VNPayIPNResponseDto
                    {
                        RspCode = "99",
                        Message = "Dữ liệu đầu vào bị thiếu"
                    };
                }

                var vnpay = new VnPayLibrary();
                
                foreach (string key in queryString)
                {
                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    {
                        string? value = queryString[key];
                        if (!string.IsNullOrEmpty(value))
                        {
                            vnpay.AddResponseData(key, value);
                        }
                    }
                }

                string txnRefStr = vnpay.GetResponseData("vnp_TxnRef");
                string amountStr = vnpay.GetResponseData("vnp_Amount");
                string transactionNoStr = vnpay.GetResponseData("vnp_TransactionNo");
                string vnpResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnpTransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                string? vnpSecureHash = queryString["vnp_SecureHash"];

                if (!long.TryParse(txnRefStr, out long orderId) ||
                    !long.TryParse(amountStr, out long amountInCents) ||
                    !long.TryParse(transactionNoStr, out long vnpayTranId))
                {
                    return new VNPayIPNResponseDto
                    {
                        RspCode = "99",
                        Message = "Định dạng dữ liệu không hợp lệ"
                    };
                }

                decimal vnpAmount = amountInCents / 100m;

                if (string.IsNullOrEmpty(vnpSecureHash))
                {
                    _log.Error("vnp_SecureHash bị thiếu");
                    return new VNPayIPNResponseDto
                    {
                        RspCode = "97",
                        Message = "Chữ ký không hợp lệ"
                    };
                }

                bool checkSignature = vnpay.ValidateSignature(vnpSecureHash, _vnpHashSecret);

                if (!checkSignature)
                {
                    _log.InfoFormat("Chữ ký không hợp lệ");
                    return new VNPayIPNResponseDto
                    {
                        RspCode = "97",
                        Message = "Chữ ký không hợp lệ"
                    };
                }

                // TODO: Query order from database
                var order = GetOrderFromDatabase(orderId);

                if (order == null)
                {
                    return new VNPayIPNResponseDto
                    {
                        RspCode = "01",
                        Message = "Không tìm thấy đơn hàng"
                    };
                }

                if (order.Amount != vnpAmount)
                {
                    return new VNPayIPNResponseDto
                    {
                        RspCode = "04",
                        Message = "Số tiền không hợp lệ"
                    };
                }

                if (order.Status != "0")
                {
                    return new VNPayIPNResponseDto
                    {
                        RspCode = "02",
                        Message = "Đơn hàng đã được xác nhận"
                    };
                }

                if (vnpResponseCode == "00" && vnpTransactionStatus == "00")
                {
                    _log.InfoFormat("Thanh toan thanh cong, OrderId={0}, VNPAY TranId={1}", orderId, vnpayTranId);
                    order.Status = "1";
                    order.PaymentTranId = vnpayTranId;
                    UpdateOrderInDatabase(order);
                }
                else
                {
                    _log.InfoFormat("Thanh toan loi, OrderId={0}, VNPAY TranId={1}, ResponseCode={2}", 
                        orderId, vnpayTranId, vnpResponseCode);
                    order.Status = "2";
                    UpdateOrderInDatabase(order);
                }

                return new VNPayIPNResponseDto
                {
                    RspCode = "00",
                    Message = "Xác nhận thành công"
                };
            }
            catch (Exception ex)
            {
                _log.Error("Lỗi khi xử lý IPN từ VNPay", ex);
                return new VNPayIPNResponseDto
                {
                    RspCode = "99",
                    Message = "Lỗi không xác định"
                };
            }
        }

        public bool ValidateSignature(NameValueCollection queryString, string secureHash)
        {
            if (queryString == null || string.IsNullOrEmpty(secureHash))
                return false;

            var vnpay = new VnPayLibrary();
            
            foreach (string key in queryString)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    string? value = queryString[key];
                    if (!string.IsNullOrEmpty(value))
                    {
                        vnpay.AddResponseData(key, value);
                    }
                }
            }

            return vnpay.ValidateSignature(secureHash, _vnpHashSecret);
        }

        // TODO: Implement these methods with actual database operations
        private OrderInfo? GetOrderFromDatabase(long orderId)
        {
            // Mock implementation
            _log.InfoFormat("Get order from database: OrderId={0}", orderId);
            return new OrderInfo
            {
                OrderId = orderId,
                Amount = 100000,
                Status = "0",
                OrderDesc = "Test order",
                CreatedDate = DateTime.Now
            };
        }

        private void UpdateOrderInDatabase(OrderInfo order)
        {
            // Mock implementation
            _log.InfoFormat("Update order in database: OrderId={0}, Status={1}", order.OrderId, order.Status);
        }
    }
}