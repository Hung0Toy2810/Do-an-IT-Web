namespace Backend.Model.dto.Payment.VNpay
{
    /// <summary>
    /// DTO cho request tạo payment
    /// </summary>
    public class VNPayPaymentRequestDto
    {
        public long OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderDescription { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string Locale { get; set; } = "vn";
        public string? BankCode { get; set; }
        public string OrderType { get; set; } = "other";
        public string? ExpireDate { get; set; }
        
        // Billing Info
        public string? BillingMobile { get; set; }
        public string? BillingEmail { get; set; }
        public string? BillingFullName { get; set; }
        public string? BillingAddress { get; set; }
        public string? BillingCity { get; set; }
        public string? BillingCountry { get; set; }
        
        // Invoice Info
        public string? InvoicePhone { get; set; }
        public string? InvoiceEmail { get; set; }
        public string? InvoiceCustomer { get; set; }
        public string? InvoiceAddress { get; set; }
        public string? InvoiceCompany { get; set; }
        public string? InvoiceTaxCode { get; set; }
        public string? InvoiceType { get; set; }
    }

    /// <summary>
    /// DTO cho response từ VNPay
    /// </summary>
    public class VNPayPaymentResponseDto
    {
        public long OrderId { get; set; }
        public long VnpayTranId { get; set; }
        public string ResponseCode { get; set; } = string.Empty;
        public string TransactionStatus { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string BankCode { get; set; } = string.Empty;
        public string TerminalId { get; set; } = string.Empty;
        public bool IsSuccess => ResponseCode == "00" && TransactionStatus == "00";
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho IPN callback từ VNPay
    /// </summary>
    public class VNPayIPNResponseDto
    {
        public string RspCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho OrderInfo
    /// </summary>
    public class OrderInfo
    {
        public long OrderId { get; set; }
        public decimal Amount { get; set; }
        public long PaymentTranId { get; set; }
        public string Status { get; set; } = "0"; // 0: Cho thanh toan, 1: Da thanh toan, 2: GD loi
        public string OrderDesc { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}