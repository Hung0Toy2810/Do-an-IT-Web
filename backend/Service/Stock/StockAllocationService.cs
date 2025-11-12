// Backend/Service/Stock/StockAllocationService.cs
using Backend.Model.Entity;
using Backend.SQLDbContext;
using Microsoft.EntityFrameworkCore;

namespace Backend.Service.Stock
{

    public class StockAllocationService : IStockAllocationService
    {
        private readonly SQLServerDbContext _context;

        public StockAllocationService(SQLServerDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<long> AllocateFromBatchAsync(long productId, string variantSlug, int quantity, long invoiceDetailId)
        {
            var batches = await _context.ShipmentBatches
                .Where(b => b.ProductId == productId
                         && b.VariantSlug == variantSlug
                         && b.RemainingQuantity > 0)
                .OrderBy(b => b.ImportedAt)
                .ToListAsync();

            int remaining = quantity;
            long allocatedBatchId = 0; // Sẽ trả về batch đầu tiên

            foreach (var batch in batches)
            {
                if (remaining <= 0) break;

                int allocate = Math.Min(remaining, batch.RemainingQuantity);
                batch.RemainingQuantity -= allocate;
                remaining -= allocate;

                allocatedBatchId = batch.Id; // Ghi nhớ batch được dùng
            }

            if (remaining > 0)
                return 0; // Không đủ hàng → 0 = lỗi

            // CẬP NHẬT detail với batchId thực tế
            var detail = await _context.InvoiceDetails.FindAsync(invoiceDetailId);
            if (detail == null)
                return 0;

            detail.ShipmentBatchId = allocatedBatchId; // BẮT BUỘC CÓ GIÁ TRỊ

            await _context.SaveChangesAsync();
            return allocatedBatchId; // Trả về batchId hợp lệ
        }

        public async Task ReleaseFromBatchAsync(long invoiceDetailId)
        {
            var detail = await _context.InvoiceDetails
                .Include(d => d.ShipmentBatch)
                .FirstOrDefaultAsync(d => d.Id == invoiceDetailId);

            if (detail?.ShipmentBatch == null)
                return;

            detail.ShipmentBatch.RemainingQuantity += detail.Quantity;

            // Không gán null → giữ nguyên hoặc xóa dòng này
            // detail.ShipmentBatchId = 0; // Nếu muốn đánh dấu đã hoàn
            await _context.SaveChangesAsync();
        }
    }
}