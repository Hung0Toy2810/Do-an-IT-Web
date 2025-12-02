// Backend/Service/AdministratorAdmin/IAdministratorAdminService.cs
using Backend.Model.dto.AdministratorAdminDtos;

namespace Backend.Service.AdministratorAdmin
{
    public interface IAdministratorAdminService
    {
        Task<PagedResult<AdministratorAdminDto>> GetAdministratorsAsync(
            string? search = null,
            bool? status = null,
            int page = 1,
            int pageSize = 20);

        Task<AdministratorAdminDto?> GetAdministratorByIdAsync(Guid id);

        Task<bool> ToggleStatusAsync(Guid id, Guid currentAdminId); // truyền ID người đang thao tác
    }
}