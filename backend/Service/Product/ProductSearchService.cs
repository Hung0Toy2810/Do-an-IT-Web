using Backend.Model.dto.Product;
using Backend.Model.Nosql;
using Backend.Repository.Product;

namespace Backend.Service.Product
{
    public interface IProductSearchService
    {
        Task<List<long>> SearchProductIdsByKeywordAsync(string keyword);
        Task<List<ProductDocument>> SearchProductsWithFiltersAsync(
            List<long> productIds, 
            decimal? minPrice, 
            decimal? maxPrice);
        Task<List<ProductDocument>> GetProductsByIdsWithBrandFilterAsync(
            List<long> productIds, 
            string? brand,
            decimal? minPrice,
            decimal? maxPrice);
        Task<List<string>> GetBrandsByProductIdsAsync(List<long> productIds);
        Task<List<ProductDocument>> GetProductsWithAdvancedFiltersAsync(
            List<long> productIds,
            List<string>? brands,
            decimal? minPrice,
            decimal? maxPrice,
            bool? inStock,
            bool? onSale);
    }

    public class ProductSearchService : IProductSearchService
    {
        private readonly IProductSearchRepository _repository;

        public ProductSearchService(IProductSearchRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<long>> SearchProductIdsByKeywordAsync(string keyword)
        {
            return await _repository.SearchProductIdsByKeywordAsync(keyword);
        }

        public async Task<List<ProductDocument>> SearchProductsWithFiltersAsync(
            List<long> productIds,
            decimal? minPrice,
            decimal? maxPrice)
        {
            return await _repository.SearchProductsWithFiltersAsync(productIds, minPrice, maxPrice);
        }

        public async Task<List<ProductDocument>> GetProductsByIdsWithBrandFilterAsync(
            List<long> productIds,
            string? brand,
            decimal? minPrice,
            decimal? maxPrice)
        {
            return await _repository.GetProductsByIdsWithBrandFilterAsync(
                productIds, brand, minPrice, maxPrice);
        }

        public async Task<List<string>> GetBrandsByProductIdsAsync(List<long> productIds)
        {
            return await _repository.GetBrandsByProductIdsAsync(productIds);
        }

        public async Task<List<ProductDocument>> GetProductsWithAdvancedFiltersAsync(
            List<long> productIds,
            List<string>? brands,
            decimal? minPrice,
            decimal? maxPrice,
            bool? inStock,
            bool? onSale)
        {
            return await _repository.GetProductsWithAdvancedFiltersAsync(
                productIds, brands, minPrice, maxPrice, inStock, onSale);
        }
    }
}