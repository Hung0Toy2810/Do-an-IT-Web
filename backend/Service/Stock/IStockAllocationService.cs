// Backend/Service/Stock/IStockAllocationService.cs
namespace Backend.Service.Stock
{
    public interface IStockAllocationService
    {
        Task<long> AllocateFromBatchAsync(long productId, string variantSlug, int quantity, long invoiceDetailId);
        Task ReleaseFromBatchAsync(long invoiceDetailId);
    }
}