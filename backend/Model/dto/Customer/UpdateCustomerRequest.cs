using System.ComponentModel.DataAnnotations;

namespace Backend.Model.dto.Customer
{
    public class UpdateCustomerRequest
    {
        [MaxLength(100, ErrorMessage = "Customer name cannot exceed 100 characters.")]
        public string? CustomerName { get; set; }

        [MaxLength(100, ErrorMessage = "Delivery address cannot exceed 100 characters.")]
        public string? DeliveryAddress { get; set; }

        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
    }
}