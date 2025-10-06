using System.ComponentModel.DataAnnotations;

namespace Backend.Model.dto.Administrator
{
    public class CreateAdministrator
    {
        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(100, ErrorMessage = "Username cannot exceed 100 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", ErrorMessage = "Password must be at least 8 characters long and contain both letters and numbers.")]

        public string Password { get; set; } = string.Empty;
    }
}