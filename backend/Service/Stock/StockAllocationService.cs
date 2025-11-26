using Backend.Model.Entity;
using Backend.Repository;
using Backend.Repository.InvoiceDetailRepository;
using Backend.SQLDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Service.Stock
{

    public class StockAllocationService : IStockAllocationService
    {
        private readonly IShipmentBatchRepository _batchRepo;
        private readonly IInvoiceDetailRepository _detailRepo;
        private readonly SQLServerDbContext _dbContext;
        private readonly ILogger<StockAllocationService> _logger;

        public StockAllocationService(
            IShipmentBatchRepository batchRepo,
            IInvoiceDetailRepository detailRepo,
            SQLServerDbContext dbContext,
            ILogger<StockAllocationService> logger)
        {
            _batchRepo = batchRepo;
            _detailRepo = detailRepo;
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Xuất hàng từ lô theo FIFO
        /// </summary>
        public async Task<long> AllocateFromBatchAsync(long productId, string variantSlug, int quantity, long invoiceDetailId)
        {
            var batches = await _batchRepo.GetAvailableBatchesByProductAndVariantAsync(productId, variantSlug);

            int remaining = quantity;
            ShipmentBatch? primaryBatch = null;

            foreach (var batch in batches)
            {
                if (remaining <= 0) break;

                int toAllocate = Math.Min(batch.RemainingQuantity, remaining);
                batch.RemainingQuantity -= toAllocate;
                remaining -= toAllocate;

                await _batchRepo.UpdateAsync(batch);

                // Lưu lô đầu tiên làm primary batch (để gán vào InvoiceDetail)
                primaryBatch ??= batch;

                _logger.LogInformation("Xuất {Qty} từ lô {BatchCode} (còn {Remaining})", 
                    toAllocate, batch.BatchCode, batch.RemainingQuantity);
            }

            if (remaining > 0)
            {
                _logger.LogError("Không đủ hàng: cần {Need}, thiếu {Missing}", quantity, remaining);
                return -1;
            }

            return primaryBatch?.Id ?? -1;
        }

        /// <summary>
        /// Giải phóng hàng (khi chưa giao) - KHÔNG hoàn vào lô, chỉ xóa liên kết
        /// Dùng khi: Hủy đơn trước khi giao, timeout thanh toán
        /// </summary>
        public async Task ReleaseFromBatchAsync(long invoiceDetailId)
        {
            var detail = await _detailRepo.GetByIdAsync(invoiceDetailId);
            if (detail == null || detail.ShipmentBatchId == null)
            {
                _logger.LogWarning("InvoiceDetail {Id} không có ShipmentBatchId để hoàn kho", invoiceDetailId);
                return;
            }

            var batch = await _dbContext.ShipmentBatches
                .FirstOrDefaultAsync(b => b.Id == detail.ShipmentBatchId.Value);

            if (batch == null)
            {
                _logger.LogError("Không tìm thấy ShipmentBatch Id = {BatchId}", detail.ShipmentBatchId);
                return;
            }

            // HOÀN KHO: tăng số lượng còn lại
            batch.RemainingQuantity += detail.Quantity;

            // Cách 1: Dùng DbContext (an toàn nhất)
            _dbContext.ShipmentBatches.Update(batch); // vẫn ok
            // hoặc nếu sợ không track: _dbContext.Attach(batch).Property(x => x.RemainingQuantity).IsModified = true;

            // QUAN TRỌNG: KHÔNG XÓA ShipmentBatchId → giữ lại để tra cứu lịch sử
            // detail.ShipmentBatchId = null;   ← XÓA DÒNG NÀY ĐI
            // _dbContext.InvoiceDetails.Update(detail); ← cũng không cần nữa

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation(
                "HOÀN KHO THÀNH CÔNG → Detail {DetailId} | +{Qty} về lô {BatchCode} | Còn lại: {Remaining} | ProductId: {ProductId}-{Variant}",
                invoiceDetailId, detail.Quantity, batch.BatchCode, batch.RemainingQuantity, 
                detail.ProductId, detail.VariantSlug);
        }

        /// <summary>
        /// Nhập hàng lại vào ĐÚNG lô cũ (khi giao thất bại)
        /// Dùng khi: Đơn đã Shipped nhưng bị Cancelled
        /// </summary>
        public async Task<bool> ReturnToBatchAsync(long invoiceDetailId)
        {
            var detail = await _detailRepo.GetByIdAsync(invoiceDetailId);
            if (detail?.ShipmentBatchId == null)
            {
                _logger.LogWarning("Detail {Id} không có BatchId để hoàn hàng", invoiceDetailId);
                return false;
            }

            var batch = await _dbContext.ShipmentBatches.FindAsync(detail.ShipmentBatchId.Value);
            if (batch == null)
            {
                _logger.LogError("Không tìm thấy lô {BatchId} để hoàn hàng", detail.ShipmentBatchId);
                return false;
            }

            // Nhập lại vào đúng lô cũ
            batch.RemainingQuantity += detail.Quantity;
            _dbContext.ShipmentBatches.Update(batch);
            await _dbContext.SaveChangesAsync();

            _logger.LogWarning("🔄 HOÀN HÀNG: Nhập {Qty} vào lô {BatchCode} (DetailId: {DetailId}, Remaining: {Remaining})", 
                detail.Quantity, batch.BatchCode, invoiceDetailId, batch.RemainingQuantity);

            return true;
        }
    }
}