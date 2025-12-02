// Service/Product/ProductService.cs
using Backend.Model.dto.Product;
using Backend.Model.Entity;
using Backend.SQLDbContext;
using Microsoft.EntityFrameworkCore;
using Backend.Repository.Product;
using Microsoft.Extensions.Logging.Abstractions;


namespace Backend.Service.Product
{
    public interface IProductService
    {
        Task<ProductDetailDto> CreateProductAsync(CreateProductDto dto);
        Task<ProductSearchResultDto> SearchProductsAsync(ProductSearchRequestDto request);
        Task<SubCategoryProductResultDto> GetProductsBySubCategoryAsync(SubCategoryProductRequestDto request);
        Task<SubCategoryBrandResultDto> GetBrandsBySubCategoryAsync(string subCategorySlug);
        Task<ProductFilterResultDto> GetProductsWithAdvancedFiltersAsync(ProductFilterRequestDto request);
        Task<ProductDetailDto?> GetProductDetailByIdAsync(long productId);
        Task<ProductDetailDto?> GetProductDetailBySlugAsync(string productSlug);
        Task<ProductCardDto?> GetProductCardByIdAsync(long productId);
        Task<ProductCardDto?> GetProductCardBySlugAsync(string productSlug);
        Task<VariantInfoDto?> GetVariantInfoAsync(long productId, string variantSlug);
        Task<VariantInfoDto?> GetVariantInfoAsync(string productSlug, string variantSlug);

        Task<List<string>> UpdateVariantImagesAsync(string productSlug, string variantSlug, List<IFormFile> images);
        Task<bool> UpdateVariantPriceAsync(string productSlug, string variantSlug, decimal originalPrice, decimal discountedPrice);
        Task<BulkOperationResultDto> BulkUpdatePricesAsync(List<BulkPriceUpdateDto> updates);
        Task<bool> UpdateIsDiscontinuedAsync(string productSlug, bool isDiscontinued);
        Task<ProductSearchResultDto> SearchAllProductsAsync(ProductSearchAllRequestDto request);

    }

    public class ProductService : IProductService
    {
        private readonly SQLServerDbContext _dbContext;
        private readonly IProductDocumentService _productDocumentService;
        private readonly IProductRepository _productRepository;
        private readonly IProductSearchService _productSearchService;
        private readonly IProductStockService _productStockService;
        // logger
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            SQLServerDbContext dbContext,
            IProductDocumentService productDocumentService,
            IProductRepository productRepository,
            ILogger<ProductService>? logger,
            IProductStockService productStockService,
            IProductSearchService productSearchService)
            

        {
            _dbContext = dbContext;
            _productDocumentService = productDocumentService;
            _productRepository = productRepository;
            _productSearchService = productSearchService;
            _logger = logger ?? NullLogger<ProductService>.Instance;
            _productStockService = productStockService;
        }

        public async Task<ProductDetailDto> CreateProductAsync(CreateProductDto dto)
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
                    SubCategoryId = dto.SubCategoryId,
                    Rating = 0.0f,
                    TotalRatings = 0
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

                var productDocument = await _productDocumentService.CreateProductAsync(createDocumentDto);
                await transaction.CommitAsync();

                return new ProductDetailDto
                {
                    Id = productDocument.Id,
                    Name = productDocument.Name,
                    Slug = productDocument.Slug,
                    Brand = productDocument.Brand,
                    Description = productDocument.Description,
                    IsDiscontinued = productDocument.IsDiscontinued,
                    Variants = productDocument.Variants,
                    AttributeOptions = productDocument.AttributeOptions,
                    Rating = product.Rating,
                    TotalRatings = product.TotalRatings
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ProductSearchResultDto> SearchProductsAsync(ProductSearchRequestDto request)
        {
            // === B1: Tìm từ SQL (subcategory) ===
            var sqlProductIds = await _productRepository.SearchProductIdsBySubCategoryAsync(request.Keyword);

            // === B2: Tìm từ MongoDB (full-text) ===
            var mongoProductIds = await _productSearchService.SearchProductIdsByKeywordAsync(request.Keyword);

            // === B3: Gộp ID ===
            var allProductIds = sqlProductIds
                .Union(mongoProductIds)
                .Distinct()
                .ToList();

            // === DEBUG: Log để kiểm tra ===
            Console.WriteLine($"[SEARCH DEBUG] Keyword: '{request.Keyword}'");
            Console.WriteLine($"[SEARCH DEBUG] SQL IDs: [{string.Join(", ", sqlProductIds)}]");
            Console.WriteLine($"[SEARCH DEBUG] Mongo IDs: [{string.Join(", ", mongoProductIds)}]");
            Console.WriteLine($"[SEARCH DEBUG] Union IDs: [{string.Join(", ", allProductIds)}]");

            if (allProductIds.Count == 0)
            {
                Console.WriteLine("[SEARCH DEBUG] No IDs found → return empty");
                return new ProductSearchResultDto
                {
                    ProductIds = new List<long>(),
                    TotalCount = 0
                };
            }

            // === B4: Lấy sản phẩm từ MongoDB (chỉ những ID có tồn tại) ===
            var products = await _productSearchService.SearchProductsWithFiltersAsync(
                allProductIds,
                request.MinPrice,
                request.MaxPrice
            );

            Console.WriteLine($"[SEARCH DEBUG] After filter → Found {products.Count} products in MongoDB");

            if (products.Count == 0)
            {
                Console.WriteLine("[SEARCH DEBUG] No products in MongoDB → return empty");
                return new ProductSearchResultDto
                {
                    ProductIds = new List<long>(),
                    TotalCount = 0
                };
            }

            // === B5: Tính giá nhỏ nhất ===
            var productsWithMinPrice = products
                .Select(p => new
                {
                    p.Id,
                    MinPrice = p.Variants.Any()
                        ? p.Variants.Min(v => v.DiscountedPrice)
                        : decimal.MaxValue
                })
                .ToList();

            // === B6: Sắp xếp ===
            IEnumerable<long> sortedProductIds;

            if (request.SortByPriceAscending.HasValue)
            {
                sortedProductIds = request.SortByPriceAscending.Value
                    ? productsWithMinPrice.OrderBy(x => x.MinPrice).Select(x => x.Id)
                    : productsWithMinPrice.OrderByDescending(x => x.MinPrice).Select(x => x.Id);
            }
            else
            {
                // Ưu tiên: SQL trước → MongoDB sau
                var sqlSet = sqlProductIds.ToHashSet();
                sortedProductIds = products
                    .OrderBy(p => sqlSet.Contains(p.Id) ? 0 : 1)
                    .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(p => p.Id);
            }

            var resultIds = sortedProductIds.ToList();

            Console.WriteLine($"[SEARCH DEBUG] Final ProductIds: [{string.Join(", ", resultIds)}]");

            return new ProductSearchResultDto
            {
                ProductIds = resultIds,
                TotalCount = resultIds.Count
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

        public async Task<ProductDetailDto?> GetProductDetailByIdAsync(long productId)
        {
            var productDocument = await _productDocumentService.GetProductDetailByIdAsync(productId);
            if (productDocument == null) return null;

            var sqlProduct = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId);

            // DÙNG _productStockService ĐỂ LẤY AVAILABLE STOCK
            var availableStock = await _productStockService.GetAvailableStockByVariantsAsync(productDocument.Slug);

            foreach (var variant in productDocument.Variants)
            {
                variant.Stock = availableStock.GetValueOrDefault(variant.Slug, 0);
            }

            return new ProductDetailDto
            {
                Id = productDocument.Id,
                Name = productDocument.Name,
                Slug = productDocument.Slug,
                Brand = productDocument.Brand,
                Description = productDocument.Description,
                IsDiscontinued = productDocument.IsDiscontinued,
                Variants = productDocument.Variants,
                AttributeOptions = productDocument.AttributeOptions,
                Rating = sqlProduct?.Rating ?? 0.0f,
                TotalRatings = sqlProduct?.TotalRatings ?? 0
            };
        }

        public async Task<ProductDetailDto?> GetProductDetailBySlugAsync(string productSlug)
        {
            var productDocument = await _productDocumentService.GetProductDetailBySlugAsync(productSlug);
            if (productDocument == null) return null;

            var sqlProduct = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productDocument.Id);

            var availableStock = await _productStockService.GetAvailableStockByVariantsAsync(productSlug);

            foreach (var variant in productDocument.Variants)
            {
                variant.Stock = availableStock.GetValueOrDefault(variant.Slug, 0);
            }

            return new ProductDetailDto
            {
                Id = productDocument.Id,
                Name = productDocument.Name,
                Slug = productDocument.Slug,
                Brand = productDocument.Brand,
                Description = productDocument.Description,
                IsDiscontinued = productDocument.IsDiscontinued,
                Variants = productDocument.Variants,
                AttributeOptions = productDocument.AttributeOptions,
                Rating = sqlProduct?.Rating ?? 0.0f,
                TotalRatings = sqlProduct?.TotalRatings ?? 0
            };
        }

        public async Task<ProductCardDto?> GetProductCardByIdAsync(long productId)
        {
            // Gọi ProductDocumentService để lấy product card từ MongoDB
            var productCard = await _productDocumentService.GetProductCardByIdAsync(productId);
            if (productCard == null) return null;

            // Lấy Rating và TotalRatings từ SQL dựa trên Id
            var sqlProduct = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId);

            return new ProductCardDto
            {
                Id = productCard.Id,
                Name = productCard.Name,
                Slug = productCard.Slug,
                Brand = productCard.Brand,
                FirstImage = productCard.FirstImage,
                MinDiscountedPrice = productCard.MinDiscountedPrice,
                OriginalPriceOfMinVariant = productCard.OriginalPriceOfMinVariant,
                IsDiscontinued = productCard.IsDiscontinued,
                Rating = sqlProduct?.Rating ?? 0.0f,
                TotalRatings = sqlProduct?.TotalRatings ?? 0
            };
        }

        public async Task<ProductCardDto?> GetProductCardBySlugAsync(string productSlug)
        {
            // Gọi ProductDocumentService để lấy product card từ MongoDB
            var productCard = await _productDocumentService.GetProductCardBySlugAsync(productSlug);
            if (productCard == null) return null;

            // Lấy Rating và TotalRatings từ SQL dựa trên Id
            var sqlProduct = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productCard.Id);

            return new ProductCardDto
            {
                Id = productCard.Id,
                Name = productCard.Name,
                Slug = productCard.Slug,
                Brand = productCard.Brand,
                FirstImage = productCard.FirstImage,
                MinDiscountedPrice = productCard.MinDiscountedPrice,
                OriginalPriceOfMinVariant = productCard.OriginalPriceOfMinVariant,
                IsDiscontinued = productCard.IsDiscontinued,
                Rating = sqlProduct?.Rating ?? 0.0f,
                TotalRatings = sqlProduct?.TotalRatings ?? 0
            };
        }

        public async Task<VariantInfoDto?> GetVariantInfoAsync(long productId, string variantSlug)
        {
            // Gọi ProductDocumentService để lấy variant info từ MongoDB
            var variantInfo = await _productDocumentService.GetVariantInfoAsync(productId, variantSlug);
            if (variantInfo == null) return null;

            // Lấy Rating và TotalRatings từ SQL dựa trên Id
            var sqlProduct = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId);

            return new VariantInfoDto
            {
                ProductId = variantInfo.ProductId,
                ProductSlug = variantInfo.ProductSlug,
                ProductName = variantInfo.ProductName,
                FirstImage = variantInfo.FirstImage,
                Attributes = variantInfo.Attributes,
                OriginalPrice = variantInfo.OriginalPrice,
                DiscountedPrice = variantInfo.DiscountedPrice,
                Rating = sqlProduct?.Rating ?? 0.0f,
                TotalRatings = sqlProduct?.TotalRatings ?? 0
            };
        }

        public async Task<VariantInfoDto?> GetVariantInfoAsync(string productSlug, string variantSlug)
        {
            // Gọi ProductDocumentService để lấy variant info từ MongoDB
            var variantInfo = await _productDocumentService.GetVariantInfoAsync(productSlug, variantSlug);
            if (variantInfo == null) return null;

            // Lấy Rating và TotalRatings từ SQL dựa trên Id
            var sqlProduct = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == variantInfo.ProductId);

            return new VariantInfoDto
            {
                ProductId = variantInfo.ProductId,
                ProductSlug = variantInfo.ProductSlug,
                ProductName = variantInfo.ProductName,
                FirstImage = variantInfo.FirstImage,
                Attributes = variantInfo.Attributes,
                OriginalPrice = variantInfo.OriginalPrice,
                DiscountedPrice = variantInfo.DiscountedPrice,
                Rating = sqlProduct?.Rating ?? 0.0f,
                TotalRatings = sqlProduct?.TotalRatings ?? 0
            };
        }

        // Các phương thức mới
        public async Task<List<string>> UpdateVariantImagesAsync(string productSlug, string variantSlug, List<IFormFile> images)
        {
            // Chuyển tiếp tới ProductDocumentService
            return await _productDocumentService.UpdateVariantImagesAsync(productSlug, variantSlug, images);
        }

        public async Task<bool> UpdateVariantPriceAsync(string productSlug, string variantSlug, decimal originalPrice, decimal discountedPrice)
        {
            // Chuyển tiếp tới ProductDocumentService
            return await _productDocumentService.UpdateVariantPriceAsync(productSlug, variantSlug, originalPrice, discountedPrice);
        }

        public async Task<BulkOperationResultDto> BulkUpdatePricesAsync(List<BulkPriceUpdateDto> updates)
        {
            // Chuyển tiếp tới ProductDocumentService
            return await _productDocumentService.BulkUpdatePricesAsync(updates);
        }

        public async Task<bool> UpdateIsDiscontinuedAsync(string productSlug, bool isDiscontinued)
        {
            // Chuyển tiếp tới ProductDocumentService
            return await _productDocumentService.UpdateIsDiscontinuedAsync(productSlug, isDiscontinued);
        }
        // Trong ProductService.cs - Thêm using Microsoft.Extensions.Logging; nếu cần ILogger
        public async Task<ProductSearchResultDto> SearchAllProductsAsync(ProductSearchAllRequestDto request)
        {
            // Validate SubCategorySlug
            if (string.IsNullOrWhiteSpace(request.SubCategorySlug))
            {
                return new ProductSearchResultDto
                {
                    ProductIds = new List<long>(),
                    TotalCount = 0
                };
            }

            var subCategoryId = await _productRepository.GetSubCategoryIdBySlugAsync(request.SubCategorySlug);
            if (!subCategoryId.HasValue)
            {
                throw new Backend.Exceptions.NotFoundException(
                    $"Không tìm thấy danh mục phụ với slug '{request.SubCategorySlug}'");
            }

            // ✅ DÙNG METHOD CŨ - Giống customer API
            var productIds = await _productRepository
                .GetProductIdsBySubCategorySlugAsync(request.SubCategorySlug);

            if (productIds.Count == 0)
            {
                return new ProductSearchResultDto
                {
                    ProductIds = new List<long>(),
                    TotalCount = 0
                };
            }

            // ✅ KHÁC API CŨ: Dùng SearchAllProductsWithFiltersAsync thay vì GetProductsByIdsWithBrandFilterAsync
            // Method này KHÔNG filter IsDiscontinued trong MongoDB
            var products = await _productSearchService
                .SearchAllProductsWithFiltersAsync(
                    productIds, 
                    request.Brand, 
                    request.MinPrice, 
                    request.MaxPrice);

            if (products.Count == 0)
            {
                return new ProductSearchResultDto
                {
                    ProductIds = new List<long>(),
                    TotalCount = 0
                };
            }

            // ✅ GIỐNG API CŨ: Logic tính MinPrice và sort
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

            return new ProductSearchResultDto
            {
                ProductIds = sortedProductIds,
                TotalCount = sortedProductIds.Count
            };
        }

        // ================================================================
        // 2. Helper method: FilterAndSortAsync (đã đổi tên tham số)
        // ================================================================
        private async Task<ProductSearchResultDto> FilterAndSortAsync(
            IReadOnlyCollection<long> productIds,          
            ProductSearchAllRequestDto request)
        {
            if (!productIds.Any())
            {
                return new ProductSearchResultDto
                {
                    ProductIds = new List<long>(),
                    TotalCount = 0
                };
            }

            var products = await _productSearchService.SearchAllProductsWithFiltersAsync(
                productIds.ToList(),
                request.Brand,
                request.MinPrice,
                request.MaxPrice);

            if (!products.Any())
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

            IEnumerable<long> sortedIds = request.SortByPriceAscending.HasValue
                ? request.SortByPriceAscending.Value
                    ? productsWithMinPrice.OrderBy(p => p.MinPrice).Select(p => p.ProductId)
                    : productsWithMinPrice.OrderByDescending(p => p.MinPrice).Select(p => p.ProductId)
                : productsWithMinPrice.Select(p => p.ProductId);

            var resultIds = sortedIds.ToList();

            return new ProductSearchResultDto
            {
                ProductIds = resultIds,
                TotalCount = resultIds.Count
            };
        }
    }
}