using Backend.Model.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Repository.InvoiceDetailRepository
{
    public interface IInvoiceDetailRepository
    {
        Task AddRangeAsync(IEnumerable<InvoiceDetail> details);
        Task<List<InvoiceDetail>> GetByInvoiceIdAsync(long invoiceId);
        Task<InvoiceDetail?> GetByIdAsync(long detailId);
        Task<bool> UpdateShipmentBatchIdAsync(long detailId, long shipmentBatchId);
        Task DeleteByInvoiceIdAsync(long invoiceId);
        Task<List<Invoice>> GetInvoicesByStatusAsync(int status);
    }
}