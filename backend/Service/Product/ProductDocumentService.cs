// CRUD cơ bản
using Backend.Model.dto.Product;
using Backend.Model.Nosql;
using Backend.Repository.Product;
using Backend.Validators;
using Backend.Repository.MinIO;

namespace Backend.Service.Product
{
    public interface IProductDocumentService
    {
        Task<ProductDocument> CreateProductAsync(CreateProductDocumentDto dto);
        Task<ProductDocument?> GetProductDetailByIdAsync(long productId);
        Task<ProductDocument?> GetProductDetailBySlugAsync(string productSlug);
        Task<ProductCardDto?> GetProductCardByIdAsync(long productId);
        Task<ProductCardDto?> GetProductCardBySlugAsync(string productSlug);
        Task<VariantInfoDto?> GetVariantInfoAsync(long productId, string variantSlug);
        Task<VariantInfoDto?> GetVariantInfoAsync(string productSlug, string variantSlug);
        Task<List<string>> UpdateVariantImagesAsync(string productSlug, string variantSlug, List<IFormFile> images);
        Task<bool> UpdateVariantPriceAsync(string productSlug, string variantSlug, decimal originalPrice, decimal discountedPrice);
        Task<BulkOperationResultDto> BulkUpdatePricesAsync(List<BulkPriceUpdateDto> updates);
        Task<bool> UpdateIsDiscontinuedAsync(string productSlug, bool isDiscontinued);
    }

    public class ProductDocumentService : IProductDocumentService
    {
        private readonly IProductDocumentRepository _repository;
        private readonly ProductDocumentValidator _validator;
        private readonly IFileRepository _fileRepository;
        private readonly IConfiguration _configuration;
        private const string ProductImagesBucket = "product-images";
        private const long MaxImageSize = 2 * 1024 * 1024;

        public ProductDocumentService(
            IProductDocumentRepository repository,
            IFileRepository fileRepository,
            IConfiguration configuration)
        {
            _repository = repository;
            _validator = new ProductDocumentValidator();
            _fileRepository = fileRepository;
            _configuration = configuration;
        }

        public async Task<ProductDocument> CreateProductAsync(CreateProductDocumentDto dto)
        {
            _validator.Validate(dto);

            // Check if product already exists
            if (await _repository.ExistsAsync(dto.Id))
            {
                throw new InvalidOperationException($"Product with Id {dto.Id} already exists");
            }

            if (await _repository.ExistsBySlugAsync(dto.Slug))
            {
                throw new InvalidOperationException($"Product with Slug '{dto.Slug}' already exists");
            }

            var document = new ProductDocument
            {
                Id = dto.Id,
                Name = dto.Name,
                Slug = dto.Slug,
                Brand = dto.Brand,
                Description = dto.Description,
                AttributeOptions = dto.AttributeOptions,
                IsDiscontinued = false,
                Variants = dto.Variants.Select(v => new ProductVariant
                {
                    Slug = v.Slug,
                    Attributes = v.Attributes,
                    Stock = 0,
                    OriginalPrice = v.OriginalPrice,
                    DiscountedPrice = v.DiscountedPrice,
                    Images = new List<string>(),
                    Specifications = v.Specifications.Select(s => new Specification
                    {
                        Label = s.Label,
                        Value = s.Value
                    }).ToList()
                }).ToList()
            };

            return await _repository.CreateAsync(document);
        }

        public async Task<List<string>> UpdateVariantImagesAsync(string productSlug, string variantSlug, List<IFormFile> images)
        {
            if (images == null || images.Count == 0)
                throw new ArgumentException("Images list cannot be null or empty", nameof(images));

            if (images.Count > 10)
                throw new ArgumentException("Maximum 10 images allowed per variant", nameof(images));

            var product = await _repository.GetBySlugAsync(productSlug);
            if (product == null)
                throw new Backend.Exceptions.NotFoundException($"Product with slug '{productSlug}' not found");

            var variant = product.Variants.FirstOrDefault(v => v.Slug == variantSlug);
            if (variant == null)
                throw new Backend.Exceptions.NotFoundException($"Variant with slug '{variantSlug}' not found in product '{productSlug}'");

            var oldImageUrls = new List<string>(variant.Images);
            var uploadedFileKeys = new List<string>();
            var newImageUrls = new List<string>();

            try
            {
                for (int i = 0; i < images.Count; i++)
                {
                    var image = images[i];
                    if (image == null || image.Length == 0)
                        throw new ArgumentException($"Image at index {i} is null or empty");

                    var fileName = $"{productSlug}_{variantSlug}_{Guid.NewGuid()}";
                    
                    using var stream = image.OpenReadStream();
                    var fileKey = await _fileRepository.ConvertAndUploadPublicFileAsJpgAsync(
                        stream, 
                        ProductImagesBucket, 
                        fileName, 
                        MaxImageSize);
                    
                    uploadedFileKeys.Add(fileKey);
                    
                    var url = await _fileRepository.GetStaticPublicFileUrl(ProductImagesBucket, fileKey);
                    newImageUrls.Add(url);
                }

                variant.Images = newImageUrls;
                bool updateSuccess = await _repository.UpdateAsync(product);

                if (!updateSuccess)
                    throw new InvalidOperationException("Failed to update product in database");

                await DeleteOldImagesAsync(oldImageUrls);

                return newImageUrls;
            }
            catch
            {
                await DeleteUploadedFilesAsync(uploadedFileKeys);
                throw;
            }
        }

        public async Task<bool> UpdateVariantPriceAsync(string productSlug, string variantSlug, decimal originalPrice, decimal discountedPrice)
        {
            if (originalPrice < 0)
                throw new ArgumentException("Original price must be greater than or equal to 0", nameof(originalPrice));

            if (discountedPrice < 0)
                throw new ArgumentException("Discounted price must be greater than or equal to 0", nameof(discountedPrice));

            if (discountedPrice > originalPrice)
                throw new ArgumentException("Discounted price cannot be greater than original price");

            var product = await _repository.GetBySlugAsync(productSlug);
            if (product == null)
                throw new Backend.Exceptions.NotFoundException($"Product with slug '{productSlug}' not found");

            var variant = product.Variants.FirstOrDefault(v => v.Slug == variantSlug);
            if (variant == null)
                throw new Backend.Exceptions.NotFoundException($"Variant with slug '{variantSlug}' not found in product '{productSlug}'");

            variant.OriginalPrice = originalPrice;
            variant.DiscountedPrice = discountedPrice;

            return await _repository.UpdateAsync(product);
        }

        public async Task<bool> UpdateIsDiscontinuedAsync(string productSlug, bool isDiscontinued)
        {
            var product = await _repository.GetBySlugAsync(productSlug);
            if (product == null)
                throw new Backend.Exceptions.NotFoundException($"Product with slug '{productSlug}' not found");

            product.IsDiscontinued = isDiscontinued;

            return await _repository.UpdateAsync(product);
        }

        private async Task DeleteOldImagesAsync(List<string> oldImageUrls)
        {
            if (oldImageUrls == null || oldImageUrls.Count == 0)
                return;

            var minioPublicUrl = _configuration["Minio:PublicUrl"];
            var prefix = $"{minioPublicUrl}/{ProductImagesBucket}/";

            foreach (var url in oldImageUrls)
            {
                try
                {
                    if (!string.IsNullOrEmpty(url) && url.StartsWith(prefix))
                    {
                        var fileKey = url.Replace(prefix, "");
                        await _fileRepository.DeleteFileAsync(ProductImagesBucket, fileKey);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to delete old image {url}: {ex.Message}");
                }
            }
        }

        public async Task<ProductDocument?> GetProductDetailByIdAsync(long productId)
        {
            return await _repository.GetByIdAsync(productId);
        }

        public async Task<ProductDocument?> GetProductDetailBySlugAsync(string productSlug)
        {
            return await _repository.GetBySlugAsync(productSlug);
        }

        public async Task<ProductCardDto?> GetProductCardByIdAsync(long productId)
        {
            var product = await _repository.GetByIdAsync(productId);
            return product == null ? null : MapToProductCard(product);
        }

        public async Task<ProductCardDto?> GetProductCardBySlugAsync(string productSlug)
        {
            var product = await _repository.GetBySlugAsync(productSlug);
            return product == null ? null : MapToProductCard(product);
        }

        private ProductCardDto MapToProductCard(ProductDocument product)
        {
            var minVariant = product.Variants
                .OrderBy(v => v.DiscountedPrice)
                .FirstOrDefault();

            if (minVariant == null)
            {
                throw new InvalidOperationException($"Product {product.Id} has no variants");
            }

            return new ProductCardDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Brand = product.Brand,
                FirstImage = minVariant.Images.FirstOrDefault(),
                MinDiscountedPrice = minVariant.DiscountedPrice,
                OriginalPriceOfMinVariant = minVariant.OriginalPrice,
                IsDiscontinued = product.IsDiscontinued
            };
        }

        public async Task<VariantInfoDto?> GetVariantInfoAsync(long productId, string variantSlug)
        {
            var product = await _repository.GetByIdAsync(productId);
            if (product == null) return null;

            var variant = product.Variants.FirstOrDefault(v => v.Slug == variantSlug);
            if (variant == null) return null;

            return new VariantInfoDto
            {
                ProductId = product.Id,
                ProductSlug = product.Slug,
                ProductName = product.Name,
                FirstImage = variant.Images.FirstOrDefault(),
                Attributes = variant.Attributes
            };
        }

        public async Task<VariantInfoDto?> GetVariantInfoAsync(string productSlug, string variantSlug)
        {
            var product = await _repository.GetBySlugAsync(productSlug);
            if (product == null) return null;

            var variant = product.Variants.FirstOrDefault(v => v.Slug == variantSlug);
            if (variant == null) return null;

            return new VariantInfoDto
            {
                ProductId = product.Id,
                ProductSlug = product.Slug,
                ProductName = product.Name,
                FirstImage = variant.Images.FirstOrDefault(),
                Attributes = variant.Attributes
            };
        }
        public async Task<BulkOperationResultDto> BulkUpdatePricesAsync(List<BulkPriceUpdateDto> updates)
        {
            var result = new BulkOperationResultDto();
            var validUpdates = new List<(long ProductId, string VariantSlug, decimal OriginalPrice, decimal DiscountedPrice)>();

            foreach (var update in updates)
            {
                try
                {
                    if (update.DiscountedPrice > update.OriginalPrice)
                    {
                        result.FailedCount++;
                        result.Errors.Add($"Discounted price > original price: {update.ProductSlug}/{update.VariantSlug}");
                        continue;
                    }

                    var product = await _repository.GetBySlugAsync(update.ProductSlug);
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

                    validUpdates.Add((product.Id, update.VariantSlug, update.OriginalPrice, update.DiscountedPrice));
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"Error: {ex.Message}");
                }
            }

            if (validUpdates.Count > 0)
            {
                result.SuccessCount = validUpdates.Count;
            }

            return result;
        }

        private async Task DeleteUploadedFilesAsync(List<string> fileKeys)
        {
            foreach (var fileKey in fileKeys)
            {
                try
                {
                    await _fileRepository.DeleteFileAsync(ProductImagesBucket, fileKey);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to delete uploaded file {fileKey}: {ex.Message}");
                }
            }
        }
    }
}