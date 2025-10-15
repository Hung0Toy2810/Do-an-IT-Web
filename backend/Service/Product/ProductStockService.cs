using Backend.Model.dto.Product;
using Backend.Model.Nosql;
using Backend.Repository.Product;

namespace Backend.Service.Product
{
    public interface IProductStockService
    {
        Task<bool> IncreaseStockAsync(string productSlug, string variantSlug, int quantity);
        Task<bool> DecreaseStockAsync(string productSlug, string variantSlug, int quantity);
        Task<bool> SetStockAsync(string productSlug, string variantSlug, int stock);
        Task<bool> ReserveStockAsync(string productSlug, string variantSlug, int quantity, string orderId, int expirationMinutes = 15);
        Task<bool> ReleaseStockAsync(string orderId);
        Task<bool> ConfirmStockReservationAsync(string orderId);
        Task<Dictionary<string, int>> GetAvailableStockByVariantsAsync(string productSlug);
        Task<BulkOperationResultDto> BulkUpdateStockAsync(List<BulkStockUpdateDto> updates);
    }

    public class ProductStockService : IProductStockService
    {
        private readonly IProductDocumentRepository _productRepository;
        private readonly IStockReservationRepository _reservationRepository;

        public ProductStockService(
            IProductDocumentRepository productRepository,
            IStockReservationRepository reservationRepository)
        {
            _productRepository = productRepository;
            _reservationRepository = reservationRepository;
        }

        public async Task<bool> IncreaseStockAsync(string productSlug, string variantSlug, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

            var product = await _productRepository.GetBySlugAsync(productSlug);
            if (product == null)
                throw new Backend.Exceptions.NotFoundException($"Product with slug '{productSlug}' not found");

            var variant = product.Variants.FirstOrDefault(v => v.Slug == variantSlug);
            if (variant == null)
                throw new Backend.Exceptions.NotFoundException($"Variant with slug '{variantSlug}' not found");

            variant.Stock += quantity;
            return await _productRepository.UpdateAsync(product);
        }

        public async Task<bool> DecreaseStockAsync(string productSlug, string variantSlug, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

            var product = await _productRepository.GetBySlugAsync(productSlug);
            if (product == null)
                throw new Backend.Exceptions.NotFoundException($"Product with slug '{productSlug}' not found");

            var variant = product.Variants.FirstOrDefault(v => v.Slug == variantSlug);
            if (variant == null)
                throw new Backend.Exceptions.NotFoundException($"Variant with slug '{variantSlug}' not found");

            if (variant.Stock < quantity)
                throw new Backend.Exceptions.BusinessRuleException($"Insufficient stock. Current: {variant.Stock}, Requested: {quantity}");

            variant.Stock -= quantity;
            return await _productRepository.UpdateAsync(product);
        }

        public async Task<bool> SetStockAsync(string productSlug, string variantSlug, int stock)
        {
            if (stock < 0)
                throw new ArgumentException("Stock must be greater than or equal to 0", nameof(stock));

            var product = await _productRepository.GetBySlugAsync(productSlug);
            if (product == null)
                throw new Backend.Exceptions.NotFoundException($"Product with slug '{productSlug}' not found");

            var variant = product.Variants.FirstOrDefault(v => v.Slug == variantSlug);
            if (variant == null)
                throw new Backend.Exceptions.NotFoundException($"Variant with slug '{variantSlug}' not found");

            variant.Stock = stock;
            return await _productRepository.UpdateAsync(product);
        }

        public async Task<bool> ReserveStockAsync(string productSlug, string variantSlug, int quantity, string orderId, int expirationMinutes = 15)
        {
            var product = await _productRepository.GetBySlugAsync(productSlug);
            if (product == null)
                throw new Backend.Exceptions.NotFoundException($"Product '{productSlug}' not found");

            var variant = product.Variants.FirstOrDefault(v => v.Slug == variantSlug);
            if (variant == null)
                throw new Backend.Exceptions.NotFoundException($"Variant '{variantSlug}' not found");

            if (variant.Stock < quantity)
                throw new Backend.Exceptions.BusinessRuleException($"Insufficient stock. Available: {variant.Stock}");

            var reservation = new StockReservation
            {
                ProductId = product.Id,
                VariantSlug = variantSlug,
                ReservedQuantity = quantity,
                OrderId = orderId,
                ReservedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Status = "Reserved"
            };

            await _reservationRepository.CreateAsync(reservation);
            variant.Stock -= quantity;
            return await _productRepository.UpdateAsync(product);
        }

        public async Task<bool> ReleaseStockAsync(string orderId)
        {
            var reservation = await _reservationRepository.GetByOrderIdAsync(orderId);
            if (reservation == null) return false;

            var product = await _productRepository.GetByIdAsync(reservation.ProductId);
            if (product == null) return false;

            var variant = product.Variants.FirstOrDefault(v => v.Slug == reservation.VariantSlug);
            if (variant == null) return false;

            variant.Stock += reservation.ReservedQuantity;
            await _productRepository.UpdateAsync(product);
            await _reservationRepository.UpdateStatusAsync(orderId, "Released");
            return true;
        }

        public async Task<bool> ConfirmStockReservationAsync(string orderId)
        {
            return await _reservationRepository.UpdateStatusAsync(orderId, "Confirmed");
        }

        public async Task<Dictionary<string, int>> GetAvailableStockByVariantsAsync(string productSlug)
        {
            var product = await _productRepository.GetBySlugAsync(productSlug);
            if (product == null)
                throw new Backend.Exceptions.NotFoundException($"Product '{productSlug}' not found");

            return product.Variants.ToDictionary(v => v.Slug, v => v.Stock);
        }

        public async Task<BulkOperationResultDto> BulkUpdateStockAsync(List<BulkStockUpdateDto> updates)
        {
            var result = new BulkOperationResultDto();
            var validUpdates = new List<(long, string, int)>();

            foreach (var update in updates)
            {
                try
                {
                    var product = await _productRepository.GetBySlugAsync(update.ProductSlug);
                    if (product == null)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Product '{update.ProductSlug}' not found");
                        continue;
                    }

                    var variant = product.Variants.FirstOrDefault(v => v.Slug == update.VariantSlug);
                    if (variant == null)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Variant '{update.VariantSlug}' not found");
                        continue;
                    }

                    validUpdates.Add((product.Id, update.VariantSlug, update.StockChange));
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"Error: {ex.Message}");
                }
            }

            if (validUpdates.Count > 0)
            {
                var stockRepo = new ProductStockRepository(null!);
                result.SuccessCount = await stockRepo.BulkUpdateStockAsync(validUpdates);
            }

            return result;
        }
    }
}