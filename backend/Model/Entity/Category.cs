using System.ComponentModel.DataAnnotations;

namespace Backend.Model.Entity
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Slug { get; set; } = string.Empty;

        public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }
}
