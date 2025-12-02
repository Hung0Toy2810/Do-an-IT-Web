using Backend.Model.Entity;
using Backend.SQLDbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Service.Product;
using Backend.Service.Checkout;
namespace Backend.Repository.InvoiceRepository
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly SQLServerDbContext _context;

        public InvoiceRepository(SQLServerDbContext context)
        {
            _context = context;
        }

        public async Task<long> CreateInvoiceAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice.Id;
        }

        public async Task AddInvoiceDetailsAsync(IEnumerable<InvoiceDetail> details)
        {
            _context.InvoiceDetails.AddRange(details);
            await _context.SaveChangesAsync();
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(long invoiceId, bool includeDetails = true, bool includePayment = true)
        {
            var query = _context.Invoices.AsQueryable();
            if (includeDetails) query = query.Include(i => i.InvoiceDetails);
            if (includePayment) query = query.Include(i => i.VNPayPayment);
            return await query.FirstOrDefaultAsync(i => i.Id == invoiceId);
        }

        public async Task<List<Invoice>> GetInvoicesByCustomerIdAsync(Guid customerId, int page = 1, int pageSize = 20)
        {
            return await _context.Invoices
                .Where(i => i.CustomerId == customerId)
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(i => i.InvoiceDetails)
                .Include(i => i.VNPayPayment)
                .ToListAsync();
        }

        public async Task<bool> UpdateInvoiceStatusAsync(long invoiceId, int newStatus)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) return false;

            invoice.Status = newStatus;
            invoice.UpdatedAt = DateTime.UtcNow;

            var history = new InvoiceStatusHistory
            {
                InvoiceId = invoiceId,
                Status = newStatus.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            _context.InvoiceStatusHistories.Add(history);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateVNPayPaymentAsync(VNPayPayment payment)
        {
            var existing = await _context.VNPayPayments.FirstOrDefaultAsync(p => p.InvoiceId == payment.InvoiceId);
            if (existing != null)
            {
                existing.TransactionCode = payment.TransactionCode;
                existing.Amount = payment.Amount;
                existing.IsSuccess = payment.IsSuccess;
                existing.ResponseCode = payment.ResponseCode;
                existing.Message = payment.Message;
                existing.PaidAt = payment.PaidAt;
            }
            else
            {
                _context.VNPayPayments.Add(payment);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Invoice?> GetInvoiceByTrackingCodeAsync(string trackingCode)
        {
            return await _context.Invoices
                .Include(i => i.InvoiceDetails)
                .Include(i => i.VNPayPayment)
                .FirstOrDefaultAsync(i => i.TrackingCode == trackingCode);
        }

        public async Task<bool> UpdateTrackingCodeAsync(long invoiceId, string trackingCode)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) return false;

            invoice.TrackingCode = trackingCode;
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Invoice>> GetInvoicesByStatusAsync(int status)
        {
            return await _context.Invoices
                .Where(i => i.Status == status)
                .ToListAsync();
        }
        public async Task<(List<Invoice> Invoices, int TotalCount)> GetAllInvoicesForAdminAsync(
            int page = 1,
            int pageSize = 20,
            InvoiceStatus? status = null,
            string? search = null)
        {
            var query = _context.Invoices
                .Include(i => i.InvoiceDetails)
                .Include(i => i.Customer)
                .AsNoTracking();

            // Lọc theo trạng thái
            if (status.HasValue)
                query = query.Where(i => i.Status == (int)status.Value);

            // TÌM KIẾM MẠNH: mã đơn + tên khách + số điện thoại + email
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(i =>
                    i.TrackingCode.ToLower().Contains(search) ||
                    (i.Customer != null && (
                        i.Customer.CustomerName.ToLower().Contains(search) ||
                        i.Customer.PhoneNumber.Contains(search) ||
                        (!string.IsNullOrEmpty(i.Customer.Email) && i.Customer.Email.ToLower().Contains(search))
                    ))
                );
            }

            var totalCount = await query.CountAsync();

            var invoices = await query
                .OrderByDescending(i => i.CreatedAt) 
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (invoices, totalCount);
        }
    }
}