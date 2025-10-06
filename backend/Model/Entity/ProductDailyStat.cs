using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Model.Entity
{
    public class ProductDailyStat
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public long ViewsCount { get; set; } = 0;

        [Required]
        public long PurchasesCount { get; set; } = 0;
    }
}
