// Backend/Repository/InvoiceRepository/IInvoiceRepository.cs
using Backend.Model.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Service.Checkout;
namespace Backend.Repository.InvoiceRepository
{
    public interface IInvoiceRepository
    {
        Task<long> CreateInvoiceAsync(Invoice invoice);
        Task AddInvoiceDetailsAsync(IEnumerable<InvoiceDetail> details);
        Task<Invoice?> GetInvoiceByIdAsync(long invoiceId, bool includeDetails = true, bool includePayment = true);
        Task<List<Invoice>> GetInvoicesByCustomerIdAsync(Guid customerId, int page = 1, int pageSize = 20);
        Task<bool> UpdateInvoiceStatusAsync(long invoiceId, int newStatus);
        Task<bool> UpdateVNPayPaymentAsync(VNPayPayment payment);
        Task<Invoice?> GetInvoiceByTrackingCodeAsync(string trackingCode);
        Task<bool> UpdateTrackingCodeAsync(long invoiceId, string trackingCode);
        Task<List<Invoice>> GetInvoicesByStatusAsync(int status);
        Task<(List<Invoice> Invoices, int TotalCount)> GetAllInvoicesForAdminAsync(
            int page = 1,
            int pageSize = 20,
            InvoiceStatus? status = null,
            string? search = null);
    }
}