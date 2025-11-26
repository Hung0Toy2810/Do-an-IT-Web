using Backend.Model.Entity;
using Backend.SQLDbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Repository.InvoiceDetailRepository
{
    public class InvoiceDetailRepository : IInvoiceDetailRepository
    {
        private readonly SQLServerDbContext _context;

        public InvoiceDetailRepository(SQLServerDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<InvoiceDetail> details)
        {
            _context.InvoiceDetails.AddRange(details);
            await _context.SaveChangesAsync();
        }

        public async Task<List<InvoiceDetail>> GetByInvoiceIdAsync(long invoiceId)
        {
            return await _context.InvoiceDetails
                .Where(d => d.InvoiceId == invoiceId)
                .Include(d => d.Product)
                .Include(d => d.ShipmentBatch)
                .ToListAsync();
        }

        public async Task<InvoiceDetail?> GetByIdAsync(long detailId)
        {
            return await _context.InvoiceDetails
                .Include(d => d.Product)
                .Include(d => d.ShipmentBatch)
                .FirstOrDefaultAsync(d => d.Id == detailId);
        }

        public async Task<bool> UpdateShipmentBatchIdAsync(long detailId, long shipmentBatchId)
        {
            var detail = await _context.InvoiceDetails.FindAsync(detailId);
            if (detail == null) return false;

            detail.ShipmentBatchId = shipmentBatchId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteByInvoiceIdAsync(long invoiceId)
        {
            var details = await _context.InvoiceDetails
                .Where(d => d.InvoiceId == invoiceId)
                .ToListAsync();

            if (details.Any())
            {
                _context.InvoiceDetails.RemoveRange(details);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Invoice>> GetInvoicesByStatusAsync(int status)
        {
            return await _context.Invoices
                .Where(i => i.Status == status)
                .ToListAsync();
        }
    }
}