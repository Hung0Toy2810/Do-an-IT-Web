using System.ComponentModel.DataAnnotations;

namespace Backend.Model.Entity
{
    public class Administrator
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string PasswordHash { get; set; } = string.Empty;
    }
}
