// Backend/Service/Invoice/IInvoiceService.cs
using Backend.Model.dto.InvoiceDtos;
using Backend.Service.Checkout;
namespace Backend.Service.Invoices
{
    public interface IInvoiceService
    {
        Task<GetInvoicesResponseDto> GetInvoicesByCustomerAsync(int page = 1, int pageSize = 20, InvoiceStatus? status = null);
        Task<GetInvoiceDetailResponseDto?> GetInvoiceDetailAsync(long invoiceId);

        Task<GetInvoicesResponseDto> GetInvoicesByCustomerIdAsync(
            Guid customerId, 
            int page = 1, 
            int pageSize = 20, 
            InvoiceStatus? status = null);
        Task<GetInvoicesResponseDto> GetAllInvoicesForAdminAsync(
            int page = 1,
            int pageSize = 20,
            InvoiceStatus? status = null,
            string? search = null);
        Task<GetInvoiceDetailResponseDto?> GetInvoiceDetailForAdminAsync(long invoiceId);
    }
}