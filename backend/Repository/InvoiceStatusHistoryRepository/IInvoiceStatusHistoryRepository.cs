using Backend.Model.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Repository.InvoiceStatusHistoryRepository
{
    public interface IInvoiceStatusHistoryRepository
    {
        Task AddAsync(InvoiceStatusHistory history);
        Task<List<InvoiceStatusHistory>> GetByInvoiceIdAsync(long invoiceId);
        Task<List<InvoiceStatusHistory>> GetByInvoiceIdAndStatusAsync(long invoiceId, string status);
    }
}