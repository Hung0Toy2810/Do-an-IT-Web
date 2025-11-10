using Backend.Model.Entity;
using System.Threading.Tasks;

namespace Backend.Repository.VNPayPaymentRepository
{
    public interface IVNPayPaymentRepository
    {
        Task<long> CreateAsync(VNPayPayment payment);
        Task<VNPayPayment?> GetByInvoiceIdAsync(long invoiceId);
        Task<VNPayPayment?> GetByTransactionCodeAsync(string transactionCode);
        Task<bool> UpdateAsync(VNPayPayment payment);
        Task<bool> MarkAsPaidAsync(long invoiceId, string transactionCode, DateTime paidAt);
    }
}