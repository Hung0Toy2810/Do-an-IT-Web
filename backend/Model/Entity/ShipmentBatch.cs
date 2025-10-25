using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Backend.Model.Entity
{
    public class ShipmentBatch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        // Mã lô (ví dụ: NHAP20251024-001)
        [Required, MaxLength(100)]
        public string BatchCode { get; set; } = string.Empty;

        // Sản phẩm trong lô này
        [Required]
        public long ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;

        // Số lượng nhập
        [Required]
        public int ImportedQuantity { get; set; }

        // Số lượng còn lại trong kho
        [Required]
        public int RemainingQuantity { get; set; }

        // Giá nhập (nếu cần theo dõi)
        [Precision(18, 2)]
        public decimal? ImportPrice { get; set; }

        // Ngày nhập kho
        [Required]
        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public String VariantSlug { get; set; } = string.Empty;

        // Một lô có thể được xuất cho nhiều chi tiết hoá đơn
        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
    }
}
