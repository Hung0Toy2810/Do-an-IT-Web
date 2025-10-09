namespace Backend.Model.Entity
{
    public class ShippingAddress
    {
        // Thông tin Tỉnh / Thành phố
        public int ProvinceId { get; set; }
        public string ProvinceCode { get; set; } = string.Empty;
        public string ProvinceName { get; set; } = string.Empty;

        // Thông tin Quận / Huyện
        public int DistrictId { get; set; }
        public string DistrictValue { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;

        // Thông tin Xã / Phường
        public int WardsId { get; set; }
        public string WardsName { get; set; } = string.Empty;

        // Địa chỉ cụ thể (ví dụ: số nhà, tên đường)
        public string? DetailAddress { get; set; }

    }
}
