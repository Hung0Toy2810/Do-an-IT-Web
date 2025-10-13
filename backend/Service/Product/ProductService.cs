// Service/Product/ProductService.cs
using Backend.Model.dto.Product;
using Backend.Model.Entity;
using Backend.SQLDbContext;
using Microsoft.EntityFrameworkCore;
using Backend.Repository.Product;

namespace Backend.Service.Product
{
    public interface IProductService
    {
        Task<long> CreateProductAsync(CreateProductDto dto);
        Task<ProductSearchResultDto> SearchProductsAsync(ProductSearchRequestDto request);
        Task<SubCategoryProductResultDto> GetProductsBySubCategoryAsync(SubCategoryProductRequestDto request);
        Task<SubCategoryBrandResultDto> GetBrandsBySubCategoryAsync(string subCategorySlug);
    }

    public class ProductService : IProductService
    {
        private readonly SQLServerDbContext _dbContext;
        private readonly IProductDocumentService _productDocumentService;
        private readonly IProductRepository _productRepository;

        public ProductService(
            SQLServerDbContext dbContext,
            IProductDocumentService productDocumentService,
            IProductRepository productRepository)
        {
            _dbContext = dbContext;
            _productDocumentService = productDocumentService;
            _productRepository = productRepository;
        }

        public async Task<long> CreateProductAsync(CreateProductDto dto)
        {
            var subCategoryExists = await _dbContext.SubCategories
                .AnyAsync(sc => sc.Id == dto.SubCategoryId);

            if (!subCategoryExists)
            {
                throw new Backend.Exceptions.NotFoundException(
                    $"SubCategory with Id {dto.SubCategoryId} not found");
            }

            var slugExists = await _productDocumentService.GetProductDetailBySlugAsync(dto.Slug);
            if (slugExists != null)
            {
                throw new InvalidOperationException(
                    $"Product with Slug '{dto.Slug}' already exists");
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var product = new Backend.Model.Entity.Product
                {
                    SubCategoryId = dto.SubCategoryId
                };

                _dbContext.Products.Add(product);
                await _dbContext.SaveChangesAsync();

                var createDocumentDto = new CreateProductDocumentDto
                {
                    Id = product.Id,
                    Name = dto.Name,
                    Slug = dto.Slug,
                    Brand = dto.Brand,
                    Description = dto.Description,
                    AttributeOptions = dto.AttributeOptions,
                    Variants = dto.Variants
                };

                await _productDocumentService.CreateProductAsync(createDocumentDto);
                await transaction.CommitAsync();

                return product.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductSearchResultDto> SearchProductsAsync(ProductSearchRequestDto request)
        {
            var sqlProductIds = await _productRepository
                .SearchProductIdsBySubCategoryAsync(request.Keyword);

            var mongoProductIds = await _productDocumentService
                .SearchProductIdsByKeywordAsync(request.Keyword);

            var allProductIds = sqlProductIds
                .Union(mongoProductIds)
                .Distinct()
                .ToList();

            if (allProductIds.Count == 0)
            {
                return new ProductSearchResultDto
                {
                    ProductIds = new List<long>(),
                    TotalCount = 0
                };
            }

            var products = await _productDocumentService
                .SearchProductsWithFiltersAsync(allProductIds, request.MinPrice, request.MaxPrice);

            if (products.Count == 0)
            {
                return new ProductSearchResultDto
                {
                    ProductIds = new List<long>(),
                    TotalCount = 0
                };
            }

            var productsWithMinPrice = products
                .Select(p => new ProductWithMinPrice
                {
                    ProductId = p.Id,
                    MinPrice = p.Variants.Any() 
                        ? p.Variants.Min(v => v.DiscountedPrice) 
                        : decimal.MaxValue
                })
                .ToList();

            IEnumerable<long> sortedProductIds;

            if (request.SortByPriceAscending.HasValue)
            {
                sortedProductIds = request.SortByPriceAscending.Value
                    ? productsWithMinPrice.OrderBy(p => p.MinPrice).Select(p => p.ProductId)
                    : productsWithMinPrice.OrderByDescending(p => p.MinPrice).Select(p => p.ProductId);
            }
            else
            {
                sortedProductIds = SortByRelevanceWithPriceMix(productsWithMinPrice);
            }

            var sortedList = sortedProductIds.ToList();

            return new ProductSearchResultDto
            {
                ProductIds = sortedList,
                TotalCount = sortedList.Count
            };
        }

        // TÍNH NĂNG MỚI 1: Lấy sản phẩm trong SubCategory
        public async Task<SubCategoryProductResultDto> GetProductsBySubCategoryAsync(SubCategoryProductRequestDto request)
        {
            // Kiểm tra SubCategory tồn tại
            var subCategoryId = await _productRepository.GetSubCategoryIdBySlugAsync(request.SubCategorySlug);
            if (!subCategoryId.HasValue)
            {
                throw new Backend.Exceptions.NotFoundException(
                    $"SubCategory with slug '{request.SubCategorySlug}' not found");
            }

            // Lấy tất cả ProductId trong SubCategory từ SQL
            var productIds = await _productRepository
                .GetProductIdsBySubCategorySlugAsync(request.SubCategorySlug);

            if (productIds.Count == 0)
            {
                return new SubCategoryProductResultDto
                {
                    ProductIds = new List<long>(),
                    TotalCount = 0
                };
            }

            // Lấy chi tiết từ MongoDB với filter Brand và giá
            var products = await _productDocumentService
                .GetProductsByIdsWithBrandFilterAsync(
                    productIds, 
                    request.Brand, 
                    request.MinPrice, 
                    request.MaxPrice);

            if (products.Count == 0)
            {
                return new SubCategoryProductResultDto
                {
                    ProductIds = new List<long>(),
                    TotalCount = 0
                };
            }

            // Tính giá thấp nhất
            var productsWithMinPrice = products
                .Select(p => new ProductWithMinPrice
                {
                    ProductId = p.Id,
                    MinPrice = p.Variants.Any() 
                        ? p.Variants.Min(v => v.DiscountedPrice) 
                        : decimal.MaxValue
                })
                .ToList();

            // Sắp xếp theo giá (nếu có)
            List<long> sortedProductIds;

            if (request.SortByPriceAscending.HasValue)
            {
                sortedProductIds = request.SortByPriceAscending.Value
                    ? productsWithMinPrice.OrderBy(p => p.MinPrice).Select(p => p.ProductId).ToList()
                    : productsWithMinPrice.OrderByDescending(p => p.MinPrice).Select(p => p.ProductId).ToList();
            }
            else
            {
                // Không sort - giữ nguyên thứ tự
                sortedProductIds = productsWithMinPrice.Select(p => p.ProductId).ToList();
            }

            return new SubCategoryProductResultDto
            {
                ProductIds = sortedProductIds,
                TotalCount = sortedProductIds.Count
            };
        }

        // TÍNH NĂNG MỚI 2: Lấy danh sách Brand trong SubCategory
        public async Task<SubCategoryBrandResultDto> GetBrandsBySubCategoryAsync(string subCategorySlug)
        {
            // Kiểm tra SubCategory tồn tại
            var subCategoryId = await _productRepository.GetSubCategoryIdBySlugAsync(subCategorySlug);
            if (!subCategoryId.HasValue)
            {
                throw new Backend.Exceptions.NotFoundException(
                    $"SubCategory with slug '{subCategorySlug}' not found");
            }

            // Lấy tất cả ProductId trong SubCategory
            var productIds = await _productRepository
                .GetProductIdsBySubCategorySlugAsync(subCategorySlug);

            if (productIds.Count == 0)
            {
                return new SubCategoryBrandResultDto
                {
                    Brands = new List<string>()
                };
            }

            // Lấy danh sách Brand từ MongoDB
            var brands = await _productDocumentService.GetBrandsByProductIdsAsync(productIds);

            return new SubCategoryBrandResultDto
            {
                Brands = brands
            };
        }

        private List<long> SortByRelevanceWithPriceMix(List<ProductWithMinPrice> productsWithMinPrice)
        {
            var orderedByPrice = productsWithMinPrice
                .OrderByDescending(p => p.MinPrice)
                .ToList();

            var totalProducts = orderedByPrice.Count;
            var highPriceCount = Math.Min(10, totalProducts);

            var highPriceProducts = orderedByPrice.Take(highPriceCount).ToList();
            var remainingProducts = orderedByPrice.Skip(highPriceCount).ToList();

            var result = new List<long>();
            var highIndex = 0;
            var lowIndex = remainingProducts.Count - 1;

            while (highIndex < highPriceProducts.Count && result.Count < 10)
            {
                result.Add(highPriceProducts[highIndex].ProductId);
                highIndex++;
            }

            while (highIndex < highPriceProducts.Count || lowIndex >= 0)
            {
                if (highIndex < highPriceProducts.Count)
                {
                    result.Add(highPriceProducts[highIndex].ProductId);
                    highIndex++;
                }

                if (lowIndex >= 0)
                {
                    result.Add(remainingProducts[lowIndex].ProductId);
                    lowIndex--;
                }
            }

            return result;
        }

        private class ProductWithMinPrice
        {
            public long ProductId { get; set; }
            public decimal MinPrice { get; set; }
        }
    }
}