using Backend.Model.Entity;
using Backend.Repository.CategoryRepository;
using Backend.Model.dto.Category;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Backend.Service.CategoryService
{
    public interface ICategoryService
    {
        Task CreateCategoryAsync(CreateCategoryDto createCategory);
        Task<CategoryDto> GetCategoryByIdAsync(long id);
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task UpdateCategoryAsync(long id, UpdateCategoryDto updateCategory);
        Task DeleteCategoryAsync(long id);
        Task CreateSubCategoryAsync(CreateSubCategoryDto createSubCategory);
        Task<SubCategoryDto> GetSubCategoryByIdAsync(long id);
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
                throw new ArgumentException("Category name cannot be empty.", nameof(createCategory.Name));

            if (await _categoryRepository.IsCategoryNameTakenAsync(createCategory.Name))
                throw new InvalidOperationException("A category with this name already exists.");

            var category = new Category
            {
                Name = createCategory.Name
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
                throw;
            }
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(long id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
                throw new ArgumentException("Category not found.");

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                SubCategories = category.SubCategories.Select(sc => new SubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
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
                SubCategories = c.SubCategories.Select(sc => new SubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    CategoryId = sc.CategoryId
                }).ToList()
            }).ToList();
        }

        public async Task UpdateCategoryAsync(long id, UpdateCategoryDto updateCategory)
        {
            if (updateCategory == null || string.IsNullOrWhiteSpace(updateCategory.Name))
                throw new ArgumentException("Category name cannot be empty.", nameof(updateCategory.Name));

            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
                throw new ArgumentException("Category not found.");

            if (await _categoryRepository.IsCategoryNameTakenAsync(updateCategory.Name) && updateCategory.Name != category.Name)
                throw new InvalidOperationException("A category with this name already exists.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                category.Name = updateCategory.Name;
                await _categoryRepository.UpdateCategoryAsync(category);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteCategoryAsync(long id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
                throw new ArgumentException("Category not found.");

            if (category.SubCategories.Any())
                throw new InvalidOperationException("Cannot delete category with existing subcategories.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _categoryRepository.DeleteCategoryAsync(id);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CreateSubCategoryAsync(CreateSubCategoryDto createSubCategory)
        {
            if (createSubCategory == null || string.IsNullOrWhiteSpace(createSubCategory.Name))
                throw new ArgumentException("SubCategory name cannot be empty.", nameof(createSubCategory.Name));

            var category = await _categoryRepository.GetCategoryByIdAsync(createSubCategory.CategoryId);
            if (category == null)
                throw new ArgumentException("Category not found.");

            if (await _categoryRepository.IsSubCategoryNameTakenAsync(createSubCategory.CategoryId, createSubCategory.Name))
                throw new InvalidOperationException("A subcategory with this name already exists in the category.");

            var subCategory = new SubCategory
            {
                Name = createSubCategory.Name,
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
                throw;
            }
        }

        public async Task<SubCategoryDto> GetSubCategoryByIdAsync(long id)
        {
            var subCategory = await _categoryRepository.GetSubCategoryByIdAsync(id);
            if (subCategory == null)
                throw new ArgumentException("SubCategory not found.");

            return new SubCategoryDto
            {
                Id = subCategory.Id,
                Name = subCategory.Name,
                CategoryId = subCategory.CategoryId
            };
        }

        public async Task<List<SubCategoryDto>> GetSubCategoriesByCategoryIdAsync(long categoryId)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(categoryId);
            if (category == null)
                throw new ArgumentException("Category not found.");

            var subCategories = await _categoryRepository.GetSubCategoriesByCategoryIdAsync(categoryId);
            return subCategories.Select(sc => new SubCategoryDto
            {
                Id = sc.Id,
                Name = sc.Name,
                CategoryId = sc.CategoryId
            }).ToList();
        }

        public async Task UpdateSubCategoryAsync(long id, UpdateSubCategoryDto updateSubCategory)
        {
            if (updateSubCategory == null || string.IsNullOrWhiteSpace(updateSubCategory.Name))
                throw new ArgumentException("SubCategory name cannot be empty.", nameof(updateSubCategory.Name));

            var subCategory = await _categoryRepository.GetSubCategoryByIdAsync(id);
            if (subCategory == null)
                throw new ArgumentException("SubCategory not found.");

            var category = await _categoryRepository.GetCategoryByIdAsync(updateSubCategory.CategoryId);
            if (category == null)
                throw new ArgumentException("Category not found.");

            if (await _categoryRepository.IsSubCategoryNameTakenAsync(updateSubCategory.CategoryId, updateSubCategory.Name) && updateSubCategory.Name != subCategory.Name)
                throw new InvalidOperationException("A subcategory with this name already exists in the category.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                subCategory.Name = updateSubCategory.Name;
                subCategory.CategoryId = updateSubCategory.CategoryId;
                await _categoryRepository.UpdateSubCategoryAsync(subCategory);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteSubCategoryAsync(long id)
        {
            var subCategory = await _categoryRepository.GetSubCategoryByIdAsync(id);
            if (subCategory == null)
                throw new ArgumentException("SubCategory not found.");

            if (subCategory.Products.Any())
                throw new InvalidOperationException("Cannot delete subcategory with existing products.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _categoryRepository.DeleteSubCategoryAsync(id);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<CategoryDto>> GetAllCategoriesWithSubCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesWithSubCategoriesAsync();
            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                SubCategories = c.SubCategories.Select(sc => new SubCategoryDto
                {
                    Id = sc.Id,
                    Name = sc.Name,
                    CategoryId = sc.CategoryId
                }).ToList()
            }).ToList();
        }
    }
}