using Backend.Model.Entity;
using Microsoft.Extensions.Logging;
using Backend.Model.Nosql.ViettelPost;

namespace Backend.Service.ViettelPost
{
    public class ViettelPostAddressValidatorService : IViettelPostAddressValidatorService
    {
        private readonly IViettelPostAddressService _addressService;
        private readonly ILogger<ViettelPostAddressValidatorService> _logger;

        public ViettelPostAddressValidatorService(
            IViettelPostAddressService addressService,
            ILogger<ViettelPostAddressValidatorService> logger)
        {
            _addressService = addressService;
            _logger = logger;
        }

        public async Task<AddressValidationResult> ValidateShippingAddressAsync(ShippingAddress address)
        {
            var result = new AddressValidationResult();

            // 1. LẤY TỈNH TỪ MONGODB
            var province = await _addressService.GetProvinceByIdAsync(address.ProvinceId);
            if (province == null)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Tỉnh không tồn tại (ID: {address.ProvinceId})";
                return result;
            }

            // 2. LẤY QUẬN TỪ MONGODB
            var districts = await _addressService.GetDistrictsByProvinceIdAsync(address.ProvinceId);
            var district = districts.FirstOrDefault(d => d.DistrictId == address.DistrictId);
            if (district == null)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Quận/Huyện không tồn tại (ID: {address.DistrictId})";
                return result;
            }

            // 3. LẤY XÃ TỪ MONGODB
            var wards = await _addressService.GetWardsByDistrictIdAsync(address.DistrictId);
            var ward = wards.FirstOrDefault(w => w.WardId == address.WardsId);
            if (ward == null)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Xã/Phường không tồn tại (ID: {address.WardsId})";
                return result;
            }

            // 4. CHI TIẾT
            if (string.IsNullOrWhiteSpace(address.DetailAddress))
            {
                result.IsValid = false;
                result.ErrorMessage = "Địa chỉ chi tiết không được để trống";
                return result;
            }

            result.IsValid = true;
            result.Province = province;
            result.District = district;
            result.Ward = ward;

            return result;
        }
    }
}