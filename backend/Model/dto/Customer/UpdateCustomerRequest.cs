using System.ComponentModel.DataAnnotations;
using Backend.Model.dto.Shipping;

namespace Backend.Model.dto.Customer
{
    public class UpdateCustomerRequest
    {
        [MaxLength(100, ErrorMessage = "Customer name cannot exceed 100 characters.")]
        public string? CustomerName { get; set; }

        public ShippingAddressDto StandardShippingAddress { get; set; } = null!;

        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
    }
}