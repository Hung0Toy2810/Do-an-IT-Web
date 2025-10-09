using Backend.Model.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Repository.CategoryRepository
{
    public interface ICategoryRepository
    {
        Task CreateCategoryAsync(Category category);
        Task<Category?> GetCategoryByIdAsync(long id);
        Task<Category?> GetCategoryByNameAsync(string name);
        Task<List<Category>> GetAllCategoriesAsync();
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(long id);
        Task<bool> IsCategoryNameTakenAsync(string name);
        Task CreateSubCategoryAsync(SubCategory subCategory);
        Task<SubCategory?> GetSubCategoryByIdAsync(long id);
        Task<List<SubCategory>> GetSubCategoriesByCategoryIdAsync(long categoryId);
        Task UpdateSubCategoryAsync(SubCategory subCategory);
        Task DeleteSubCategoryAsync(long id);
        Task<bool> IsSubCategoryNameTakenAsync(long categoryId, string name);
        Task<List<Category>> GetAllCategoriesWithSubCategoriesAsync();
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly SQLServerDbContext _context;

        public CategoryRepository(SQLServerDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task CreateCategoryAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(long id)
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .ToListAsync();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(long id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsCategoryNameTakenAsync(string name)
        {
            return await _context.Categories.AnyAsync(c => c.Name == name);
        }

        public async Task CreateSubCategoryAsync(SubCategory subCategory)
        {
            await _context.SubCategories.AddAsync(subCategory);
            await _context.SaveChangesAsync();
        }

        public async Task<SubCategory?> GetSubCategoryByIdAsync(long id)
        {
            return await _context.SubCategories
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc => sc.Id == id);
        }

        public async Task<List<SubCategory>> GetSubCategoriesByCategoryIdAsync(long categoryId)
        {
            return await _context.SubCategories
                .Include(sc => sc.Category)
                .Where(sc => sc.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task UpdateSubCategoryAsync(SubCategory subCategory)
        {
            _context.SubCategories.Update(subCategory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSubCategoryAsync(long id)
        {
            var subCategory = await _context.SubCategories.FindAsync(id);
            if (subCategory != null)
            {
                _context.SubCategories.Remove(subCategory);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsSubCategoryNameTakenAsync(long categoryId, string name)
        {
            return await _context.SubCategories
                .AnyAsync(sc => sc.CategoryId == categoryId && sc.Name == name);
        }

        public async Task<List<Category>> GetAllCategoriesWithSubCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.SubCategories)
                .ToListAsync();
        }
    }
}