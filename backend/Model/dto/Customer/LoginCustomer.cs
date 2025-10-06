using System.ComponentModel.DataAnnotations;

namespace Backend.Model.dto.Customer
{
    public class LoginCustomer
    {
        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        [RegularExpression(@"^\+?\d{8,15}$", ErrorMessage = "Phone number must be a valid number with 8 to 15 digits.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        public string Password { get; set; } = string.Empty;
    }
}