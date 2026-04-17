

namespace PharmacyApi.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyApi.DTOs;
using PharmacyApi.Services;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Pharmacist")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDTO categoryDto)
    {
        try
        {
            var category = await _categoryService.CreateCategoryAsync(categoryDto);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDTO categoryDto)
    {
        try
        {
            var category = await _categoryService.UpdateCategoryAsync(id, categoryDto);
            return Ok(category);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a category (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        try
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result)
                return NotFound(new { error = $"Category with ID {id} not found" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound(new { error = $"Category with ID {id} not found" });

        return Ok(category);
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllCategories([FromQuery] bool includeProducts = false)
    {
        var categories = await _categoryService.GetAllCategoriesAsync(includeProducts);
        return Ok(categories);
    }

    /// <summary>
    /// Get category hierarchy tree
    /// </summary>
    [HttpGet("tree")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoryTree()
    {
        var tree = await _categoryService.GetCategoryTreeAsync();
        return Ok(tree);
    }
}
