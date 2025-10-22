using Backend.Model.Entity;
using Backend.Repository.CategoryRepository;
using Backend.Model.dto.Category;
using Backend.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Backend.Service.CategoryService
{
    public interface ICategoryService
    {
        Task CreateCategoryAsync(CreateCategoryDto createCategory);
        Task<CategoryDto> GetCategoryByIdAsync(long id);
        Task<CategoryDto> GetCategoryBySlugAsync(string slug);
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task UpdateCategoryAsync(long id, UpdateCategoryDto updateCategory);
        Task DeleteCategoryAsync(long id);
        Task CreateSubCategoryAsync(CreateSubCategoryDto createSubCategory);
        Task<SubCategoryDto> GetSubCategoryByIdAsync(long id);
        Task<SubCategoryDto> GetSubCategoryBySlugAsync(long categoryId, string slug);
        Task<List<SubCategoryDto>> GetSubCategoriesByCategoryIdAsync(long categoryId);
        Task UpdateSubCategoryAsync(long id, UpdateSubCategoryDto updateSubCategory);
        Task DeleteSubCategoryAsync(long id);
        Task<List<CategoryDto>> GetAllCategoriesWithSubCategoriesAsync();
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly SQLServerDbContext _context;

        public CategoryService(ICategoryRepository categoryRepository, SQLServerDbContext context)
        {
            _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task CreateCategoryAsync(CreateCategoryDto createCategory)
        {
            if (createCategory == null || string.IsNullOrWhiteSpace(createCategory.Name))
                throw new ArgumentException("Tên danh mục không được để trống.");

            if (await _categoryRepository.IsCategoryNameTakenAsync(createCategory.Name))
                throw new InvalidOperationException("Tên danh mục này đã được sử dụng.");

            var slug = SlugHelper.GenerateSlug(createCategory.Name);
            if (await _categoryRepository.IsCategorySlugTakenAsync(slug))
                throw new InvalidOperationException("Đường dẫn (slug) của danh mục này đã tồn tại.");

            var category = new Category
            {
                Name = createCategory.Name,
                Slug = slug
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _categoryRepository.CreateCategoryAsync(category);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new Exception("Không thể tạo danh mục. Vui lòng thử lại sau.");
            }
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(long id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
                throw new ArgumentException("Không tìm thấy danh mục.");

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                SubCategories = category.SubCategories.Select(sc => new SubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Slug = sc.Slug,
                    CategoryId = sc.CategoryId
                }).ToList()
            };
        }

        public async Task<CategoryDto> GetCategoryBySlugAsync(string slug)
        {
            var category = await _categoryRepository.GetCategoryBySlugAsync(slug);
            if (category == null)
                throw new ArgumentException("Không tìm thấy danh mục.");

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                SubCategories = category.SubCategories.Select(sc => new SubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Slug = sc.Slug,
                    CategoryId = sc.CategoryId
                }).ToList()
            };
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                SubCategories = c.SubCategories.Select(sc => new SubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Slug = sc.Slug,
                    CategoryId = sc.CategoryId
                }).ToList()
            }).ToList();
        }

        public async Task UpdateCategoryAsync(long id, UpdateCategoryDto updateCategory)
        {
            if (updateCategory == null || string.IsNullOrWhiteSpace(updateCategory.Name))
                throw new ArgumentException("Tên danh mục không được để trống.");

            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
                throw new ArgumentException("Không tìm thấy danh mục.");

            if (updateCategory.Name != category.Name)
            {
                if (await _categoryRepository.IsCategoryNameTakenAsync(updateCategory.Name))
                    throw new InvalidOperationException("Tên danh mục này đã được sử dụng.");

                var newSlug = SlugHelper.GenerateSlug(updateCategory.Name);
                if (await _categoryRepository.IsCategorySlugTakenAsync(newSlug))
                    throw new InvalidOperationException("Đường dẫn (slug) của danh mục này đã tồn tại.");

                category.Name = updateCategory.Name;
                category.Slug = newSlug;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _categoryRepository.UpdateCategoryAsync(category);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new Exception("Không thể cập nhật danh mục. Vui lòng thử lại sau.");
            }
        }

        public async Task DeleteCategoryAsync(long id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
                throw new ArgumentException("Không tìm thấy danh mục.");

            if (category.SubCategories.Any())
                throw new InvalidOperationException("Không thể xóa danh mục khi vẫn còn danh mục con.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _categoryRepository.DeleteCategoryAsync(id);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new Exception("Không thể xóa danh mục. Vui lòng thử lại sau.");
            }
        }

        public async Task CreateSubCategoryAsync(CreateSubCategoryDto createSubCategory)
        {
            if (createSubCategory == null || string.IsNullOrWhiteSpace(createSubCategory.Name))
                throw new ArgumentException("Tên danh mục con không được để trống.");

            var category = await _categoryRepository.GetCategoryByIdAsync(createSubCategory.CategoryId);
            if (category == null)
                throw new ArgumentException("Không tìm thấy danh mục cha.");

            if (await _categoryRepository.IsSubCategoryNameTakenAsync(createSubCategory.CategoryId, createSubCategory.Name))
                throw new InvalidOperationException("Tên danh mục con này đã được sử dụng trong danh mục cha.");

            var slug = SlugHelper.GenerateSlug(createSubCategory.Name);
            if (await _categoryRepository.IsSubCategorySlugTakenAsync(createSubCategory.CategoryId, slug))
                throw new InvalidOperationException("Đường dẫn (slug) của danh mục con này đã tồn tại trong danh mục cha.");

            var subCategory = new SubCategory
            {
                Name = createSubCategory.Name,
                Slug = slug,
                CategoryId = createSubCategory.CategoryId
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _categoryRepository.CreateSubCategoryAsync(subCategory);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new Exception("Không thể tạo danh mục con. Vui lòng thử lại sau.");
            }
        }

        public async Task<SubCategoryDto> GetSubCategoryByIdAsync(long id)
        {
            var subCategory = await _categoryRepository.GetSubCategoryByIdAsync(id);
            if (subCategory == null)
                throw new ArgumentException("Không tìm thấy danh mục con.");

            return new SubCategoryDto
            {
                Id = subCategory.Id,
                Name = subCategory.Name,
                Slug = subCategory.Slug,
                CategoryId = subCategory.CategoryId
            };
        }

        public async Task<SubCategoryDto> GetSubCategoryBySlugAsync(long categoryId, string slug)
        {
            var subCategory = await _categoryRepository.GetSubCategoryBySlugAsync(categoryId, slug);
            if (subCategory == null)
                throw new ArgumentException("Không tìm thấy danh mục con.");

            return new SubCategoryDto
            {
                Id = subCategory.Id,
                Name = subCategory.Name,
                Slug = subCategory.Slug,
                CategoryId = subCategory.CategoryId
            };
        }

        public async Task<List<SubCategoryDto>> GetSubCategoriesByCategoryIdAsync(long categoryId)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (category == null)
                throw new ArgumentException("Không tìm thấy danh mục.");

            var subCategories = await _categoryRepository.GetSubCategoriesByCategoryIdAsync(categoryId);
            return subCategories.Select(sc => new SubCategoryDto
            {
                Id = sc.Id,
                Name = sc.Name,
                Slug = sc.Slug,
                CategoryId = sc.CategoryId
            }).ToList();
        }

        public async Task UpdateSubCategoryAsync(long id, UpdateSubCategoryDto updateSubCategory)
        {
            if (updateSubCategory == null || string.IsNullOrWhiteSpace(updateSubCategory.Name))
                throw new ArgumentException("Tên danh mục con không được để trống.");

            var subCategory = await _categoryRepository.GetSubCategoryByIdAsync(id);
            if (subCategory == null)
                throw new ArgumentException("Không tìm thấy danh mục con.");

            var newCategory = await _categoryRepository.GetCategoryByIdAsync(updateSubCategory.CategoryId);
            if (newCategory == null)
                throw new ArgumentException("Không tìm thấy danh mục cha mới.");

            bool nameChanged = updateSubCategory.Name != subCategory.Name;
            bool categoryChanged = updateSubCategory.CategoryId != subCategory.CategoryId;

            if (nameChanged || categoryChanged)
            {
                if (await _categoryRepository.IsSubCategoryNameTakenAsync(updateSubCategory.CategoryId, updateSubCategory.Name))
                    throw new InvalidOperationException("Tên danh mục con này đã được sử dụng trong danh mục đích.");

                var newSlug = SlugHelper.GenerateSlug(updateSubCategory.Name);
                if (await _categoryRepository.IsSubCategorySlugTakenAsync(updateSubCategory.CategoryId, newSlug))
                    throw new InvalidOperationException("Đường dẫn (slug) của danh mục con này đã tồn tại trong danh mục đích.");

                subCategory.Name = updateSubCategory.Name;
                subCategory.Slug = newSlug;
                subCategory.CategoryId = updateSubCategory.CategoryId;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _categoryRepository.UpdateSubCategoryAsync(subCategory);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new Exception("Không thể cập nhật danh mục con. Vui lòng thử lại sau.");
            }
        }

        public async Task DeleteSubCategoryAsync(long id)
        {
            var subCategory = await _categoryRepository.GetSubCategoryByIdAsync(id);
            if (subCategory == null)
                throw new ArgumentException("Không tìm thấy danh mục con.");

            if (subCategory.Products.Any())
                throw new InvalidOperationException("Không thể xóa danh mục con khi vẫn còn sản phẩm.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _categoryRepository.DeleteSubCategoryAsync(id);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new Exception("Không thể xóa danh mục con. Vui lòng thử lại sau.");
            }
        }

        public async Task<List<CategoryDto>> GetAllCategoriesWithSubCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesWithSubCategoriesAsync();
            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                SubCategories = c.SubCategories.Select(sc => new SubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    Slug = sc.Slug,
                    CategoryId = sc.CategoryId
                }).ToList()
            }).ToList();
        }
    }
}
