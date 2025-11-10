using Backend.Model.Nosql.ViettelPost;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Repository.ViettelPost
{
    public interface IViettelPostAddressRepository
    {
        Task<List<ProvinceDocument>> GetAllProvincesAsync();
        Task<ProvinceDocument?> GetProvinceByIdAsync(int provinceId);
        Task<List<DistrictDocument>> GetDistrictsByProvinceIdAsync(int provinceId);
        Task<List<WardDocument>> GetWardsByDistrictIdAsync(int districtId);
        Task UpsertProvincesAsync(List<ProvinceDocument> provinces);
        Task UpsertDistrictsAsync(List<DistrictDocument> districts);
        Task UpsertWardsAsync(List<WardDocument> wards);
        Task DeleteAllProvincesAsync();
        Task DeleteAllDistrictsAsync();
        Task DeleteAllWardsAsync();
    }
}