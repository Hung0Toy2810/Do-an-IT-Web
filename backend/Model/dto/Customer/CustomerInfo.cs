using Backend.Model.dto.Shipping;
namespace Backend.Model.dto.Customer
{
    public class CustomerInfoDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public ShippingAddressDto StandardShippingAddress { get; set; } = new ShippingAddressDto();
        public string PhoneNumber { get; set; } = string.Empty;
        public string AvtURL { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}