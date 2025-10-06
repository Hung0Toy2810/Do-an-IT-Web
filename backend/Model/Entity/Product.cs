using System.ComponentModel.DataAnnotations;

namespace Backend.Model.Entity
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long SubCategoryId { get; set; }
        [ForeignKey(nameof(SubCategoryId))]
        public virtual SubCategory SubCategory { get; set; } = null!;

        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
        public virtual ICollection<RecentlyView> RecentlyViews { get; set; } = new List<RecentlyView>();
        public virtual ICollection<ProductDailyStat> ProductDailyStats { get; set; } = new List<ProductDailyStat>();
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
    }
}
