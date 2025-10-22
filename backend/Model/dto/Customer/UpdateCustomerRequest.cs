using System.ComponentModel.DataAnnotations;
using Backend.Model.dto.Shipment;

namespace Backend.Model.dto.Customer
{
    public class UpdateCustomerRequest
    {
        [MaxLength(100, ErrorMessage = "Tên khách hàng không được vượt quá 100 ký tự.")]
        public string? CustomerName { get; set; }

        public ShippingAddressDto StandardShippingAddress { get; set; } = null!;

        [MaxLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        public string? Email { get; set; }
    }
}
