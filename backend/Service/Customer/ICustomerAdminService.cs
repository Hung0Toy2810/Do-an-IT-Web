// Backend/Service/CustomerAdmin/ICustomerAdminService.cs
using Backend.Model.dto.CustomerAdminDtos;

namespace Backend.Service.CustomerAdmin
{
    public interface ICustomerAdminService
    {
        Task<PagedResult<CustomerAdminDto>> GetCustomersAsync(
            string? search = null,
            bool? status = null,
            int page = 1,
            int pageSize = 20);

        Task<CustomerAdminDto?> GetCustomerByIdAsync(Guid id);

        Task<bool> ToggleStatusAsync(Guid id);
    }
}