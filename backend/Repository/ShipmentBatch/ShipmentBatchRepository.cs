using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.SQLDbContext;
using Backend.Model.Entity;
using Backend.Exceptions;

namespace Backend.Repository
{
    public interface IShipmentBatchRepository
    {
        Task<ShipmentBatch> CreateAsync(ShipmentBatch batch);
        Task<ShipmentBatch?> GetByBatchCodeAsync(string batchCode);
        Task<List<ShipmentBatch>> GetAvailableBatchesByProductAndVariantAsync(long productId, string variantSlug);
        Task UpdateAsync(ShipmentBatch batch);
        Task<string> GenerateBatchCodeAsync(string prefix = "NHAP");
    }

    public class ShipmentBatchRepository : IShipmentBatchRepository
    {
        private readonly SQLServerDbContext _dbContext;

        public ShipmentBatchRepository(SQLServerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ShipmentBatch> CreateAsync(ShipmentBatch batch)
        {
            _dbContext.ShipmentBatches.Add(batch);
            await _dbContext.SaveChangesAsync();
            return batch;
        }

        public async Task<ShipmentBatch?> GetByBatchCodeAsync(string batchCode)
        {
            return await _dbContext.ShipmentBatches
                .Include(b => b.Product)
                .FirstOrDefaultAsync(b => b.BatchCode == batchCode);
        }

        public async Task<List<ShipmentBatch>> GetAvailableBatchesByProductAndVariantAsync(long productId, string variantSlug)
        {
            return await _dbContext.ShipmentBatches
                .Where(b => b.ProductId == productId && b.VariantSlug == variantSlug && b.RemainingQuantity > 0)
                .OrderBy(b => b.ImportedAt) // FIFO: nhập trước xuất trước
                .ToListAsync();
        }

        public async Task UpdateAsync(ShipmentBatch batch)
        {
            _dbContext.ShipmentBatches.Update(batch);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<string> GenerateBatchCodeAsync(string prefix = "NHAP")
        {
            var today = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _dbContext.ShipmentBatches
                .CountAsync(b => b.BatchCode.StartsWith($"{prefix}{today}-")) + 1;
            return $"{prefix}{today}-{count:D3}";
        }
    }
}