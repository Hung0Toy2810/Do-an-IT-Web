using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.SQLDbContext;
using Backend.Model.Entity;
using Backend.Repository;
using Backend.Model.Nosql;
using Backend.Exceptions;
using Backend.Repository.Product;
using Backend.Model.dto.Inventory;

namespace Backend.Service.Inventory
{
    public interface IInventoryService
    {
        Task<string> ImportStockAsync(string productSlug, string variantSlug, int quantity, decimal? importPrice = null);
        Task<bool> ExportStockAsync(string batchCode, int quantity);
        Task<List<ExportedBatchDto>> SellStockAsync(string productSlug, string variantSlug, int quantity);

        Task<List<ShipmentBatchDto>> GetBatchesByProductAndVariantAsync(string productSlug, string variantSlug);
        Task<List<ShipmentBatchDto>> GetAllBatchesByProductAndVariantAsync(string productSlug, string variantSlug);
    }

    public class InventoryService : IInventoryService
    {
        private readonly SQLServerDbContext _dbContext;
        private readonly IProductDocumentRepository _productRepository;
        private readonly IShipmentBatchRepository _shipmentBatchRepository;

        public InventoryService(
            SQLServerDbContext dbContext,
            IProductDocumentRepository productRepository,
            IShipmentBatchRepository shipmentBatchRepository)
        {
            _dbContext = dbContext;
            _productRepository = productRepository;
            _shipmentBatchRepository = shipmentBatchRepository;
        }

        public async Task<string> ImportStockAsync(string productSlug, string variantSlug, int quantity, decimal? importPrice = null)
        {
            if (quantity <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0", nameof(quantity));

            var productDoc = await _productRepository.GetBySlugAsync(productSlug);
            if (productDoc == null)
                throw new NotFoundException($"Không tìm thấy sản phẩm với slug '{productSlug}'");

            var variant = productDoc.Variants.FirstOrDefault(v => v.Slug == variantSlug);
            if (variant == null)
                throw new NotFoundException($"Không tìm thấy biến thể với slug '{variantSlug}'");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var batchCode = await _shipmentBatchRepository.GenerateBatchCodeAsync();

                var batch = new ShipmentBatch
                {
                    BatchCode = batchCode,
                    ProductId = productDoc.Id,
                    ImportedQuantity = quantity,
                    RemainingQuantity = quantity,
                    ImportPrice = importPrice,
                    ImportedAt = DateTime.UtcNow,
                    VariantSlug = variantSlug
                };

                await _shipmentBatchRepository.CreateAsync(batch);

                variant.Stock += quantity;
                var updateSuccess = await _productRepository.UpdateAsync(productDoc);
                if (!updateSuccess)
                    throw new BusinessRuleException("Cập nhật tồn kho NoSQL thất bại");

                await transaction.CommitAsync();
                return batchCode;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ExportStockAsync(string batchCode, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0", nameof(quantity));

            var batch = await _shipmentBatchRepository.GetByBatchCodeAsync(batchCode);
            if (batch == null)
                throw new NotFoundException($"Không tìm thấy lô hàng với mã '{batchCode}'");

            if (batch.RemainingQuantity < quantity)
                throw new BusinessRuleException($"Số lượng còn lại không đủ. Hiện tại: {batch.RemainingQuantity}, Yêu cầu: {quantity}");

            var productDoc = await _productRepository.GetByIdAsync(batch.ProductId);
            if (productDoc == null)
                throw new NotFoundException($"Không tìm thấy sản phẩm với ID {batch.ProductId}");

            var variant = productDoc.Variants.FirstOrDefault(v => v.Slug == batch.VariantSlug);
            if (variant == null)
                throw new NotFoundException($"Không tìm thấy biến thể với slug '{batch.VariantSlug}'");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                batch.RemainingQuantity -= quantity;
                await _shipmentBatchRepository.UpdateAsync(batch);

                variant.Stock -= quantity;
                var updateSuccess = await _productRepository.UpdateAsync(productDoc);
                if (!updateSuccess)
                    throw new BusinessRuleException("Cập nhật tồn kho NoSQL thất bại");

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<ExportedBatchDto>> SellStockAsync(string productSlug, string variantSlug, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0", nameof(quantity));

            var productDoc = await _productRepository.GetBySlugAsync(productSlug);
            if (productDoc == null)
                throw new NotFoundException($"Không tìm thấy sản phẩm với slug '{productSlug}'");

            var variant = productDoc.Variants.FirstOrDefault(v => v.Slug == variantSlug);
            if (variant == null)
                throw new NotFoundException($"Không tìm thấy biến thể với slug '{variantSlug}'");

            if (variant.Stock < quantity)
                throw new BusinessRuleException($"Tồn kho không đủ. Hiện tại: {variant.Stock}, Yêu cầu: {quantity}");

            var batches = await _shipmentBatchRepository.GetAvailableBatchesByProductAndVariantAsync(productDoc.Id, variantSlug);
            if (!batches.Any())
                throw new BusinessRuleException("Không có lô hàng khả dụng cho sản phẩm này");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var exported = new List<ExportedBatchDto>();
                int remainingToSell = quantity;

                foreach (var batch in batches)
                {
                    if (remainingToSell <= 0) break;

                    int take = Math.Min(batch.RemainingQuantity, remainingToSell);
                    batch.RemainingQuantity -= take;
                    await _shipmentBatchRepository.UpdateAsync(batch);

                    exported.Add(new ExportedBatchDto
                    {
                        BatchCode = batch.BatchCode,
                        ExportedQuantity = take
                    });

                    remainingToSell -= take;
                }

                if (remainingToSell > 0)
                    throw new BusinessRuleException("Không đủ lô hàng để xuất đầy đủ số lượng yêu cầu");

                variant.Stock -= quantity;
                var updateSuccess = await _productRepository.UpdateAsync(productDoc);
                if (!updateSuccess)
                    throw new BusinessRuleException("Cập nhật tồn kho NoSQL thất bại");

                await transaction.CommitAsync();
                return exported;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<ShipmentBatchDto>> GetBatchesByProductAndVariantAsync(string productSlug, string variantSlug)
        {
            var productDoc = await _productRepository.GetBySlugAsync(productSlug);
            if (productDoc == null)
                throw new NotFoundException($"Không tìm thấy sản phẩm với slug '{productSlug}'");

            var variant = productDoc.Variants.FirstOrDefault(v => v.Slug == variantSlug);
            if (variant == null)
                throw new NotFoundException($"Không tìm thấy biến thể với slug '{variantSlug}'");

            var batches = await _shipmentBatchRepository.GetAvailableBatchesByProductAndVariantAsync(productDoc.Id, variantSlug);

            return batches.Select(b => new ShipmentBatchDto
            {
                BatchCode = b.BatchCode,
                ImportedQuantity = b.ImportedQuantity,
                RemainingQuantity = b.RemainingQuantity,
                ImportPrice = b.ImportPrice,
                ImportedAt = b.ImportedAt,
                VariantSlug = b.VariantSlug
            }).ToList();
        }

        public async Task<List<ShipmentBatchDto>> GetAllBatchesByProductAndVariantAsync(string productSlug, string variantSlug)
        {
            if (string.IsNullOrWhiteSpace(productSlug))
                throw new ArgumentException("productSlug là bắt buộc", nameof(productSlug));
            if (string.IsNullOrWhiteSpace(variantSlug))
                throw new ArgumentException("variantSlug là bắt buộc", nameof(variantSlug));

            var productDoc = await _productRepository.GetBySlugAsync(productSlug);
            if (productDoc == null)
                throw new NotFoundException($"Không tìm thấy sản phẩm với slug '{productSlug}'");

            var variant = productDoc.Variants.FirstOrDefault(v => v.Slug == variantSlug);
            if (variant == null)
                throw new NotFoundException($"Không tìm thấy biến thể với slug '{variantSlug}'");

            var batches = await _shipmentBatchRepository.GetAllBatchesByProductAndVariantAsync(productDoc.Id, variantSlug);

            return batches.Select(b => new ShipmentBatchDto
            {
                BatchCode = b.BatchCode,
                ImportedQuantity = b.ImportedQuantity,
                RemainingQuantity = b.RemainingQuantity,
                ImportPrice = b.ImportPrice,
                ImportedAt = b.ImportedAt,
                VariantSlug = b.VariantSlug
            }).ToList();
        }
    }
}