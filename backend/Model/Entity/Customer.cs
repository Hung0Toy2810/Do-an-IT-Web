using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Backend.Model.Entity
{
    public class Customer
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;
        [Required]
        public ShippingAddress StandardShippingAddress { get; set; } = new ShippingAddress();

        [Required, MaxLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string HashPassword { get; set; } = string.Empty;

        [Required]
        public bool Status { get; set; } = true;

        [MaxLength(255)]
        public string AvtURL { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<RecentlyView> RecentlyViews { get; set; } = new List<RecentlyView>();
        // comment navigation property
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}