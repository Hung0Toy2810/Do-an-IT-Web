// Backend/Service/CustomerAdmin/CustomerAdminService.cs
using Backend.Model.Entity;
using Backend.Model.dto.CustomerAdminDtos;
using Backend.Repository.CustomerRepository;
using Microsoft.EntityFrameworkCore;

namespace Backend.Service.CustomerAdmin
{
    public class CustomerAdminService : ICustomerAdminService
    {
        private readonly ICustomerRepository _customerRepo;
        private readonly SQLServerDbContext _context;

        public CustomerAdminService(ICustomerRepository customerRepo, SQLServerDbContext context)
        {
            _customerRepo = customerRepo;
            _context = context;
        }

        public async Task<PagedResult<CustomerAdminDto>> GetCustomersAsync(
            string? search = null,
            bool? status = null,
            int page = 1,
            int pageSize = 20)
        {
            IQueryable<Customer> query = _context.Customers.AsNoTracking();

            // TÌM KIẾM SIÊU MẠNH
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(c =>
                    c.CustomerName.Contains(search) ||
                    c.PhoneNumber.Contains(search) ||
                    c.Email.Contains(search) ||
                    (c.CustomerName + " " + c.PhoneNumber).Contains(search) ||
                    (c.PhoneNumber + " " + c.CustomerName).Contains(search));
            }

            // LỌC TRẠNG THÁI
            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            var totalCount = await query.CountAsync();

            var customers = await query
                .OrderByDescending(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerAdminDto
                {
                    Id = c.Id,
                    CustomerName = c.CustomerName,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    AvtURL = c.AvtURL,
                    Status = c.Status,
                    TotalInvoices = c.Invoices.Count // Lazy loading tự động
                })
                .ToListAsync();

            return new PagedResult<CustomerAdminDto>
            {
                Items = customers,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CustomerAdminDto?> GetCustomerByIdAsync(Guid id)
        {
            return await _context.Customers
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CustomerAdminDto
                {
                    Id = c.Id,
                    CustomerName = c.CustomerName,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    AvtURL = c.AvtURL,
                    Status = c.Status,
                    TotalInvoices = c.Invoices.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ToggleStatusAsync(Guid id)
        {
            var customer = await _customerRepo.GetCustomerByIdAsync(id);
            if (customer == null) return false;

            customer.Status = !customer.Status;
            await _customerRepo.UpdateCustomerAsync(customer);
            return true;
        }
    }
}