namespace Backend.Model.dto.Shipping
{
    public class ShippingAddressDto
    {
        public int ProvinceId { get; set; }
        public string ProvinceCode { get; set; } = string.Empty;
        public string ProvinceName { get; set; } = string.Empty;
        public int DistrictId { get; set; }
        public string DistrictValue { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public int WardsId { get; set; }
        public string WardsName { get; set; } = string.Empty;
        public string? DetailAddress { get; set; }
    }
}