using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Model.Entity
{
    public class VNPayPayment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long InvoiceId { get; set; }
        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice Invoice { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string TransactionCode { get; set; } = string.Empty; // mã giao dịch do VNPay trả về

        [Required]
        [Precision(18, 4)]
        public decimal Amount { get; set; } // số tiền thanh toán

        [Required]
        public bool IsSuccess { get; set; } = false; // trạng thái thanh toán

        [MaxLength(255)]
        public string? ResponseCode { get; set; } // mã phản hồi từ VNPay (VD: 00 = thành công)

        [MaxLength(255)]
        public string? Message { get; set; } // mô tả lỗi hoặc thông báo

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // lúc tạo yêu cầu

        public DateTime? PaidAt { get; set; } // lúc VNPay xác nhận thành công
    }
}
