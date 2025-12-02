using Backend.Service.Checkout;
namespace Backend.Model.dto.InvoiceDtos
{
    public class InvoiceListItemDto
    {
        public long Id { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public InvoiceStatus Status { get; set; }

        public string StatusText => Status switch
        {
            InvoiceStatus.Pending => "Chờ xác nhận",
            InvoiceStatus.Paid => "Đã thanh toán",
            InvoiceStatus.Shipped => "Đang giao hàng",
            InvoiceStatus.Delivered => "Đã giao hàng",
            InvoiceStatus.Cancelled => "Đã hủy",
            InvoiceStatus.PaymentFailed => "Thanh toán thất bại",
            _ => "Không xác định"
        };

        public string StatusBadgeColor => Status switch
        {
            InvoiceStatus.Pending => "warning",
            InvoiceStatus.Paid => "info",
            InvoiceStatus.Shipped => "primary",
            InvoiceStatus.Delivered => "success",
            InvoiceStatus.Cancelled => "secondary",
            InvoiceStatus.PaymentFailed => "danger",
            _ => "dark"
        };

        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "COD";
        public int TotalItems { get; set; }
        public string? FirstProductImage { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
    }
}