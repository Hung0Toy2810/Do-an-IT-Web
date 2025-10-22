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
        Task<ProductFilterResultDto> GetProductsWithAdvancedFiltersAsync(ProductFilterRequestDto request);
    }

    public class ProductService : IProductService
    {
        private readonly SQLServerDbContext _dbContext;
        private readonly IProductDocumentService _productDocumentService;
        private readonly IProductRepository _productRepository;
        private readonly IProductSearchService _productSearchService;

        public ProductService(
            SQLServerDbContext dbContext,
            IProductDocumentService productDocumentService,
            IProductRepository productRepository,
            IProductSearchService productSearchService)
        {
            _dbContext = dbContext;
            _productDocumentService = productDocumentService;
            _productRepository = productRepository;
            _productSearchService = productSearchService;
        }

        public async Task<long> CreateProductAsync(CreateProductDto dto)
        {
            var subCategoryExists = await _dbContext.SubCategories
                .AnyAsync(sc => sc.Id == dto.SubCategoryId);

            if (!subCategoryExists)
            {
                throw new Backend.Exceptions.NotFoundException(
                    $"Không tìm thấy danh mục phụ với Id {dto.SubCategoryId}");
            }

            var slugExists = await _productDocumentService.GetProductDetailBySlugAsync(dto.Slug);
            if (slugExists != null)
            {
                throw new InvalidOperationException(
                    $"Sản phẩm với Slug '{dto.Slug}' đã tồn tại");
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

            // 🔄 ĐỔI: _productDocumentService → _productSearchService
            var mongoProductIds = await _productSearchService
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

            // 🔄 ĐỔI: _productDocumentService → _productSearchService
            var products = await _productSearchService
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

        public async Task<SubCategoryProductResultDto> GetProductsBySubCategoryAsync(SubCategoryProductRequestDto request)
        {
            var subCategoryId = await _productRepository.GetSubCategoryIdBySlugAsync(request.SubCategorySlug);
            if (!subCategoryId.HasValue)
            {
                throw new Backend.Exceptions.NotFoundException(
                    $"Không tìm thấy danh mục phụ với slug '{request.SubCategorySlug}'");
            }

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

            // 🔄 ĐỔI: _productDocumentService → _productSearchService
            var products = await _productSearchService
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

            var productsWithMinPrice = products
                .Select(p => new ProductWithMinPrice
                {
                    ProductId = p.Id,
                    MinPrice = p.Variants.Any() 
                        ? p.Variants.Min(v => v.DiscountedPrice) 
                        : decimal.MaxValue
                })
                .ToList();

            List<long> sortedProductIds;

            if (request.SortByPriceAscending.HasValue)
            {
                sortedProductIds = request.SortByPriceAscending.Value
                    ? productsWithMinPrice.OrderBy(p => p.MinPrice).Select(p => p.ProductId).ToList()
                    : productsWithMinPrice.OrderByDescending(p => p.MinPrice).Select(p => p.ProductId).ToList();
            }
            else
            {
                sortedProductIds = productsWithMinPrice.Select(p => p.ProductId).ToList();
            }

            return new SubCategoryProductResultDto
            {
                ProductIds = sortedProductIds,
                TotalCount = sortedProductIds.Count
            };
        }

        public async Task<SubCategoryBrandResultDto> GetBrandsBySubCategoryAsync(string subCategorySlug)
        {
            var subCategoryId = await _productRepository.GetSubCategoryIdBySlugAsync(subCategorySlug);
            if (!subCategoryId.HasValue)
            {
                throw new Backend.Exceptions.NotFoundException(
                    $"Không tìm thấy danh mục phụ với slug '{subCategorySlug}'");
            }

            var productIds = await _productRepository
                .GetProductIdsBySubCategorySlugAsync(subCategorySlug);

            if (productIds.Count == 0)
            {
                return new SubCategoryBrandResultDto
                {
                    Brands = new List<string>()
                };
            }

            var brands = await _productSearchService.GetBrandsByProductIdsAsync(productIds);

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

        public async Task<ProductFilterResultDto> GetProductsWithAdvancedFiltersAsync(ProductFilterRequestDto request)
        {
            List<long> productIds = new();

            if (request.SubCategorySlugs != null && request.SubCategorySlugs.Count > 0)
            {
                foreach (var slug in request.SubCategorySlugs)
                {
                    var ids = await _productRepository.GetProductIdsBySubCategorySlugAsync(slug);
                    productIds.AddRange(ids);
                }
                productIds = productIds.Distinct().ToList();
            }
            else
            {
                var allProducts = await _productRepository.GetAllProductIdsAsync();
                productIds = allProducts;
            }

            if (productIds.Count == 0)
            {
                return new ProductFilterResultDto
                {
                    ProductIds = new List<long>(),
                    TotalCount = 0,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = 0
                };
            }

            var products = await _productSearchService.GetProductsWithAdvancedFiltersAsync(
                productIds,
                request.Brands,
                request.MinPrice,
                request.MaxPrice,
                request.InStock,
                request.OnSale);

            if (products.Count == 0)
            {
                return new ProductFilterResultDto
                {
                    ProductIds = new List<long>(),
                    TotalCount = 0,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = 0
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

            IEnumerable<long> sortedProductIds = request.SortBy switch
            {
                "price_asc" => productsWithMinPrice.OrderBy(p => p.MinPrice).Select(p => p.ProductId),
                "price_desc" => productsWithMinPrice.OrderByDescending(p => p.MinPrice).Select(p => p.ProductId),
                "newest" => productsWithMinPrice.Select(p => p.ProductId),
                _ => productsWithMinPrice.Select(p => p.ProductId)
            };

            var sortedList = sortedProductIds.ToList();
            var totalCount = sortedList.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var pagedProductIds = sortedList
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new ProductFilterResultDto
            {
                ProductIds = pagedProductIds,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages
            };
        }

        private class ProductWithMinPrice
        {
            public long ProductId { get; set; }
            public decimal MinPrice { get; set; }
        }
    }
}