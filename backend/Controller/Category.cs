using Backend.Service.CategoryService;
using Backend.Model.dto.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        /// Creates a new category.
        /// </summary>
        /// <param name="createCategory">The category data including name.</param>
        /// <returns>A response indicating the result of the creation operation.</returns>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategory)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid input data.");
            }

            await _categoryService.CreateCategoryAsync(createCategory);
            return Created($"api/categories/{createCategory.Name}", new
            {
                Message = "Category created successfully.",
                Name = createCategory.Name
            });
        }

        /// <summary>
        /// Gets a category by ID.
        /// </summary>
        /// <param name="id">The ID of the category.</param>
        /// <returns>The category information.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCategory(long id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            return Ok(category);
        }

        /// <summary>
        /// Gets all categories.
        /// </summary>
        /// <returns>A list of all categories.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Updates a category.
        /// </summary>
        /// <param name="id">The ID of the category to update.</param>
        /// <param name="updateCategory">The updated category data.</param>
        /// <returns>A response indicating the result of the update operation.</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCategory(long id, [FromBody] UpdateCategoryDto updateCategory)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid input data.");
            }

            await _categoryService.UpdateCategoryAsync(id, updateCategory);
            return Ok(new { Message = "Category updated successfully." });
        }

        /// <summary>
        /// Deletes a category.
        /// </summary>
        /// <param name="id">The ID of the category to delete.</param>
        /// <returns>A response indicating the result of the deletion operation.</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return Ok(new { Message = "Category deleted successfully." });
        }

        /// <summary>
        /// Creates a new subcategory under a category.
        /// </summary>
        /// <param name="createSubCategory">The subcategory data including name and category ID.</param>
        /// <returns>A response indicating the result of the creation operation.</returns>
        [HttpPost("subcategories")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateSubCategory([FromBody] CreateSubCategoryDto createSubCategory)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid input data.");
            }

            await _categoryService.CreateSubCategoryAsync(createSubCategory);
            return Created($"api/categories/{createSubCategory.CategoryId}/subcategories/{createSubCategory.Name}", new
            {
                Message = "SubCategory created successfully.",
                Name = createSubCategory.Name,
                CategoryId = createSubCategory.CategoryId
            });
        }

        /// <summary>
        /// Gets a subcategory by ID.
        /// </summary>
        /// <param name="id">The ID of the subcategory.</param>
        /// <returns>The subcategory information.</returns>
        [HttpGet("subcategories/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSubCategory(long id)
        {
            var subCategory = await _categoryService.GetSubCategoryByIdAsync(id);
            return Ok(subCategory);
        }

        /// <summary>
        /// Gets all subcategories under a specific category.
        /// </summary>
        /// <param name="categoryId">The ID of the category.</param>
        /// <returns>A list of subcategories.</returns>
        [HttpGet("{categoryId}/subcategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetSubCategoriesByCategoryId(long categoryId)
        {
            var subCategories = await _categoryService.GetSubCategoriesByCategoryIdAsync(categoryId);
            return Ok(subCategories);
        }

        /// <summary>
        /// Updates a subcategory.
        /// </summary>
        /// <param name="id">The ID of the subcategory to update.</param>
        /// <param name="updateSubCategory">The updated subcategory data.</param>
        /// <returns>A response indicating the result of the update operation.</returns>
        [HttpPut("subcategories/{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateSubCategory(long id, [FromBody] UpdateSubCategoryDto updateSubCategory)
        {
            if (!ModelState.IsValid)
            {
                throw new ArgumentException("Invalid input data.");
            }

            await _categoryService.UpdateSubCategoryAsync(id, updateSubCategory);
            return Ok(new { Message = "SubCategory updated successfully." });
        }

        /// <summary>
        /// Deletes a subcategory.
        /// </summary>
        /// <param name="id">The ID of the subcategory to delete.</param>
        /// <returns>A response indicating the result of the deletion operation.</returns>
        [HttpDelete("subcategories/{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteSubCategory(long id)
        {
            await _categoryService.DeleteSubCategoryAsync(id);
            return Ok(new { Message = "SubCategory deleted successfully." });
        }

        /// <summary>
        /// Gets all categories with their subcategories for frontend use.
        /// </summary>
        /// <returns>A list of all categories with their subcategories.</returns>
        [HttpGet("with-subcategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategoriesWithSubCategories()
        {
            var categories = await _categoryService.GetAllCategoriesWithSubCategoriesAsync();
            return Ok(categories);
        }
    }
}