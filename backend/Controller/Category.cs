using Backend.Model.dto.Category;
using Backend.Service.CategoryService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategory)
        {
            await _categoryService.CreateCategoryAsync(createCategory);
            return Created($"api/categories", new { Message = "Tạo danh mục thành công" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(long id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(new
            {
                Message = "Lấy thông tin danh mục thành công",
                Data = category
            });
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetCategoryBySlug(string slug)
        {
            var category = await _categoryService.GetCategoryBySlugAsync(slug);
            return Ok(new
            {
                Message = "Lấy thông tin danh mục thành công",
                Data = category
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(new
            {
                Message = "Lấy danh sách danh mục thành công",
                Data = categories
            });
        }

        [HttpGet("with-subcategories")]
        public async Task<IActionResult> GetAllCategoriesWithSubCategories()
        {
            var categories = await _categoryService.GetAllCategoriesWithSubCategoriesAsync();
            return Ok(new
            {
                Message = "Lấy danh sách danh mục thành công",
                Data = categories
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(long id, [FromBody] UpdateCategoryDto updateCategory)
        {
            await _categoryService.UpdateCategoryAsync(id, updateCategory);
            return Ok(new { Message = "Cập nhật danh mục thành công" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return Ok(new { Message = "Xóa danh mục thành công" });
        }

        [HttpPost("subcategories")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSubCategory([FromBody] CreateSubCategoryDto createSubCategory)
        {
            await _categoryService.CreateSubCategoryAsync(createSubCategory);
            return Created($"api/categories/subcategories", new { Message = "Tạo danh mục con thành công" });
        }

        [HttpGet("subcategories/{id}")]
        public async Task<IActionResult> GetSubCategoryById(long id)
        {
            var subCategory = await _categoryService.GetSubCategoryByIdAsync(id);
            return Ok(new
            {
                Message = "Lấy thông tin danh mục con thành công",
                Data = subCategory
            });
        }

        [HttpGet("{categoryId}/subcategories/slug/{slug}")]
        public async Task<IActionResult> GetSubCategoryBySlug(long categoryId, string slug)
        {
            var subCategory = await _categoryService.GetSubCategoryBySlugAsync(categoryId, slug);
            return Ok(new
            {
                Message = "Lấy thông tin danh mục con thành công",
                Data = subCategory
            });
        }

        [HttpGet("{categoryId}/subcategories")]
        public async Task<IActionResult> GetSubCategoriesByCategoryId(long categoryId)
        {
            var subCategories = await _categoryService.GetSubCategoriesByCategoryIdAsync(categoryId);
            return Ok(new
            {
                Message = "Lấy danh sách danh mục con thành công",
                Data = subCategories
            });
        }

        [HttpPut("subcategories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSubCategory(long id, [FromBody] UpdateSubCategoryDto updateSubCategory)
        {
            await _categoryService.UpdateSubCategoryAsync(id, updateSubCategory);
            return Ok(new { Message = "Cập nhật danh mục con thành công" });
        }

        [HttpDelete("subcategories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSubCategory(long id)
        {
            await _categoryService.DeleteSubCategoryAsync(id);
            return Ok(new { Message = "Xóa danh mục con thành công" });
        }
    }
}