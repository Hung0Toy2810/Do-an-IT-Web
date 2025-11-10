using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Model.Nosql.ViettelPost;

namespace Backend.Service.ViettelPost
{
    public interface IViettelPostAddressService
    {
        Task<List<Province>> GetAllProvincesAsync();
        Task<Province?> GetProvinceByIdAsync(int provinceId);
        Task<List<District>> GetDistrictsByProvinceIdAsync(int provinceId);
        Task<List<Ward>> GetWardsByDistrictIdAsync(int districtId);
    }
}