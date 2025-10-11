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

        /// <summary>
        /// Tạo category mới (auto-generate slug từ name)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategory)
        {
            await _categoryService.CreateCategoryAsync(createCategory);
            return Created($"api/categories", new { Message = "Tạo category thành công." });
        }

        /// <summary>
        /// Lấy category theo ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoryById(long id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(category);
        }

        /// <summary>
        /// Lấy category theo slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoryBySlug(string slug)
        {
            var category = await _categoryService.GetCategoryBySlugAsync(slug);
            return Ok(category);
        }

        /// <summary>
        /// Lấy tất cả categories
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Lấy tất cả categories kèm subcategories
        /// </summary>
        [HttpGet("with-subcategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategoriesWithSubCategories()
        {
            var categories = await _categoryService.GetAllCategoriesWithSubCategoriesAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Cập nhật category (auto-generate slug mới nếu name thay đổi)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(long id, [FromBody] UpdateCategoryDto updateCategory)
        {
            await _categoryService.UpdateCategoryAsync(id, updateCategory);
            return Ok(new { Message = "Cập nhật category thành công." });
        }

        /// <summary>
        /// Xóa category
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return Ok(new { Message = "Xóa category thành công." });
        }

        /// <summary>
        /// Tạo subcategory mới (auto-generate slug từ name)
        /// </summary>
        [HttpPost("subcategories")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSubCategory([FromBody] CreateSubCategoryDto createSubCategory)
        {
            await _categoryService.CreateSubCategoryAsync(createSubCategory);
            return Created($"api/categories/subcategories", new { Message = "Tạo subcategory thành công." });
        }

        /// <summary>
        /// Lấy subcategory theo ID
        /// </summary>
        [HttpGet("subcategories/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSubCategoryById(long id)
        {
            var subCategory = await _categoryService.GetSubCategoryByIdAsync(id);
            return Ok(subCategory);
        }

        /// <summary>
        /// Lấy subcategory theo category ID và slug
        /// </summary>
        [HttpGet("{categoryId}/subcategories/slug/{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSubCategoryBySlug(long categoryId, string slug)
        {
            var subCategory = await _categoryService.GetSubCategoryBySlugAsync(categoryId, slug);
            return Ok(subCategory);
        }

        /// <summary>
        /// Lấy tất cả subcategories của một category
        /// </summary>
        [HttpGet("{categoryId}/subcategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSubCategoriesByCategoryId(long categoryId)
        {
            var subCategories = await _categoryService.GetSubCategoriesByCategoryIdAsync(categoryId);
            return Ok(subCategories);
        }

        /// <summary>
        /// Cập nhật subcategory (auto-generate slug mới nếu name thay đổi)
        /// </summary>
        [HttpPut("subcategories/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSubCategory(long id, [FromBody] UpdateSubCategoryDto updateSubCategory)
        {
            await _categoryService.UpdateSubCategoryAsync(id, updateSubCategory);
            return Ok(new { Message = "Cập nhật subcategory thành công." });
        }

        /// <summary>
        /// Xóa subcategory
        /// </summary>
        [HttpDelete("subcategories/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSubCategory(long id)
        {
            await _categoryService.DeleteSubCategoryAsync(id);
            return Ok(new { Message = "Xóa subcategory thành công." });
        }
    }
}