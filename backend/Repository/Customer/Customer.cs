using Backend.Model.Entity;
using Backend.SQLDbContext;
namespace Backend.Repository.CustomerRepository
{
    public interface ICustomerRepository
    {
        Task CreateCustomerAsync(Customer customer);
        Task<Customer?> GetCustomerByPhoneNumberAsync(string phoneNumber);
        Task<Customer?> GetCustomerByIdAsync(Guid id);
        Task UpdateCustomerAsync(Customer customer);
        Task<bool> IsPhoneNumberTakenAsync(string phoneNumber);
        Task<bool> IsEmailTakenAsync(string email);
        Task<List<Customer>> GetAllCustomersAsync();

    }
    public class CustomerRepository : ICustomerRepository
    {
        private readonly SQLServerDbContext _context;

        public CustomerRepository(SQLServerDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task CreateCustomerAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<Customer?> GetCustomerByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber && c.Status);
        }

        public async Task<Customer?> GetCustomerByIdAsync(Guid id)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsPhoneNumberTakenAsync(string phoneNumber)
        {
            return await _context.Customers
                .AnyAsync(c => c.PhoneNumber == phoneNumber && c.Status);
        }

        public async Task<bool> IsEmailTakenAsync(string email)
        {
            return await _context.Customers
                .AnyAsync(c => c.Email == email && c.Status);
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers.ToListAsync();
        }
    }
}