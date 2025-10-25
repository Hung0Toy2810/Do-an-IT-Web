using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Model.Entity
{
    public class InvoiceDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long InvoiceId { get; set; }

        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice Invoice { get; set; } = null!;

        [Required]
        public long ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }
        [Required]
        [Precision(18, 2)]
        public decimal Price { get; set; }

        [Required, MaxLength(100)]
        public string VariantSlug { get; set; } = string.Empty;

        [Required]
        public long ShipmentBatchId { get; set; }
        [ForeignKey(nameof(ShipmentBatchId))]
        public virtual ShipmentBatch ShipmentBatch { get; set; } = null!;
    }
}
