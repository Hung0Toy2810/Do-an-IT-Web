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
        [Required]
        public float Rating { get; set; } = 0.0f;
        [Required]
        public long TotalRatings { get; set; } = 0;

        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
        public virtual ICollection<RecentlyView> RecentlyViews { get; set; } = new List<RecentlyView>();
        public virtual ICollection<ProductDailyStat> ProductDailyStats { get; set; } = new List<ProductDailyStat>();
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
