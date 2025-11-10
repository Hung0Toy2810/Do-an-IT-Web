using System.ComponentModel.DataAnnotations;

namespace Backend.Model.Entity
{
    public class Invoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public Guid CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; } = null!;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        [MaxLength(100)]
        public string TrackingCode { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string ReceiverName { get; set; } = string.Empty;
        [Required]
        [MaxLength(20)]
        public string ReceiverPhone { get; set; } = string.Empty;
        [Required]
        public int Status { get; set; } = 0;
        [Required]
        public ShippingAddress ShippingAddress { get; set; } = new ShippingAddress();

        [MaxLength(20)]
        public string? Carrier { get; set; } = "viettelpost";

        public DateTime? EstimatedDelivery { get; set; }

        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
        public virtual ICollection<InvoiceStatusHistory> StatusHistories { get; set; } = new List<InvoiceStatusHistory>();
        public virtual VNPayPayment? VNPayPayment { get; set; }
    }
}
