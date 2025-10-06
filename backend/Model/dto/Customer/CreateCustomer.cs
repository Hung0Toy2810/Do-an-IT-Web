using System.ComponentModel.DataAnnotations;

namespace Backend.Model.dto.Customer
{
    public class CreateCustomer
    {
        [Required(ErrorMessage = "Customer name is required.")]
        [MaxLength(100, ErrorMessage = "Customer name cannot exceed 100 characters.")]
        [RegularExpression(@"^\+?\d{8,15}$", ErrorMessage = "Phone number must be a valid number with 8 to 15 digits.")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", ErrorMessage = "Password must be at least 8 characters long and contain both letters and numbers.")]
        public string Password { get; set; } = string.Empty;

    }
}