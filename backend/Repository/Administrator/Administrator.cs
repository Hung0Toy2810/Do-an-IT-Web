using Backend.Model.Entity;
using System;
using System.Threading.Tasks;

namespace Backend.Repository.AdministratorRepository
{
    public interface IAdministratorRepository
    {
        Task CreateAdministratorAsync(Administrator administrator);
        Task<Administrator?> GetAdministratorByUsernameAsync(string username);
        Task<Administrator?> GetAdministratorByIdAsync(Guid id);
        Task UpdateAdministratorAsync(Administrator administrator);
        Task<bool> IsUsernameTakenAsync(string username);
        Task<List<Administrator>> GetAllAdministratorsAsync();
    }

    public class AdministratorRepository : IAdministratorRepository
    {
        private readonly SQLServerDbContext _context;

        public AdministratorRepository(SQLServerDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task CreateAdministratorAsync(Administrator administrator)
        {
            await _context.Administrators.AddAsync(administrator);
            await _context.SaveChangesAsync();
        }

        public async Task<Administrator?> GetAdministratorByUsernameAsync(string username)
        {
            return await _context.Administrators
                .FirstOrDefaultAsync(a => a.Username == username && a.Status);
        }

        public async Task<Administrator?> GetAdministratorByIdAsync(Guid id)
        {
            return await _context.Administrators
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task UpdateAdministratorAsync(Administrator administrator)
        {
            _context.Administrators.Update(administrator);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            return await _context.Administrators
                .AnyAsync(a => a.Username == username && a.Status);
        }

        public async Task<List<Administrator>> GetAllAdministratorsAsync()
        {
            return await _context.Administrators.ToListAsync();
        }
    }
}