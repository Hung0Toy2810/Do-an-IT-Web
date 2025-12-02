// Backend/Service/AdministratorAdmin/AdministratorAdminService.cs
using Backend.Model.Entity;
using Backend.Model.dto.AdministratorAdminDtos;
using Backend.Repository.AdministratorRepository;
using Microsoft.EntityFrameworkCore;

namespace Backend.Service.AdministratorAdmin
{
    public class AdministratorAdminService : IAdministratorAdminService
    {
        private readonly IAdministratorRepository _adminRepo;
        private readonly SQLServerDbContext _context;

        public AdministratorAdminService(IAdministratorRepository adminRepo, SQLServerDbContext context)
        {
            _adminRepo = adminRepo;
            _context = context;
        }

        public async Task<PagedResult<AdministratorAdminDto>> GetAdministratorsAsync(
            string? search = null, bool? status = null, int page = 1, int pageSize = 20)
        {
            IQueryable<Administrator> query = _context.Administrators.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(a => a.Username.Contains(search));
            }

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            var totalCount = await query.CountAsync();

            var admins = await query
                .OrderByDescending(a => a.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AdministratorAdminDto
                {
                    Id = a.Id,
                    Username = a.Username,
                    Status = a.Status
                })
                .ToListAsync();

            return new PagedResult<AdministratorAdminDto>
            {
                Items = admins,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<AdministratorAdminDto?> GetAdministratorByIdAsync(Guid id)
        {
            var admin = await _context.Administrators
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AdministratorAdminDto
                {
                    Id = a.Id,
                    Username = a.Username,
                    Status = a.Status
                })
                .FirstOrDefaultAsync();

            return admin;
        }

        public async Task<bool> ToggleStatusAsync(Guid id, Guid currentAdminId)
        {
            // NGĂN ADMIN TỰ KHÓA MÌNH
            if (id == currentAdminId)
                return false;

            var admin = await _adminRepo.GetAdministratorByIdAsync(id);
            if (admin == null) return false;

            admin.Status = !admin.Status;
            await _adminRepo.UpdateAdministratorAsync(admin);
            return true;
        }
    }
}