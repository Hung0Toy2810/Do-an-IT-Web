using Backend.Model.Entity;
using Backend.SQLDbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Repository.InvoiceStatusHistoryRepository
{
    public class InvoiceStatusHistoryRepository : IInvoiceStatusHistoryRepository
    {
        private readonly SQLServerDbContext _context;

        public InvoiceStatusHistoryRepository(SQLServerDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(InvoiceStatusHistory history)
        {
            _context.InvoiceStatusHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<List<InvoiceStatusHistory>> GetByInvoiceIdAsync(long invoiceId)
        {
            return await _context.InvoiceStatusHistories
                .Where(h => h.InvoiceId == invoiceId)
                .OrderBy(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<InvoiceStatusHistory>> GetByInvoiceIdAndStatusAsync(long invoiceId, string status)
        {
            return await _context.InvoiceStatusHistories
                .Where(h => h.InvoiceId == invoiceId && h.Status == status)
                .OrderBy(h => h.CreatedAt)
                .ToListAsync();
        }
    }
}