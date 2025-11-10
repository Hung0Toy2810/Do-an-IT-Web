using Backend.Model.Entity;
using Backend.SQLDbContext;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Backend.Repository.VNPayPaymentRepository
{
    public class VNPayPaymentRepository : IVNPayPaymentRepository
    {
        private readonly SQLServerDbContext _context;

        public VNPayPaymentRepository(SQLServerDbContext context)
        {
            _context = context;
        }

        public async Task<long> CreateAsync(VNPayPayment payment)
        {
            _context.VNPayPayments.Add(payment);
            await _context.SaveChangesAsync();
            return payment.Id;
        }

        public async Task<VNPayPayment?> GetByInvoiceIdAsync(long invoiceId)
        {
            return await _context.VNPayPayments
                .Include(p => p.Invoice)
                .FirstOrDefaultAsync(p => p.InvoiceId == invoiceId);
        }

        public async Task<VNPayPayment?> GetByTransactionCodeAsync(string transactionCode)
        {
            return await _context.VNPayPayments
                .Include(p => p.Invoice)
                .FirstOrDefaultAsync(p => p.TransactionCode == transactionCode);
        }

        public async Task<bool> UpdateAsync(VNPayPayment payment)
        {
            var existing = await _context.VNPayPayments.FindAsync(payment.Id);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsPaidAsync(long invoiceId, string transactionCode, DateTime paidAt)
        {
            var payment = await GetByInvoiceIdAsync(invoiceId);
            if (payment == null) return false;

            payment.IsSuccess = true;
            payment.TransactionCode = transactionCode;
            payment.PaidAt = paidAt;
            payment.ResponseCode = "00"; // VNPay success code
            payment.Message = "Thanh toán thành công";

            await _context.SaveChangesAsync();
            return true;
        }
    }
}