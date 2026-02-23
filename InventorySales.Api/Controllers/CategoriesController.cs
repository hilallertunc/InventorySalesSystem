using InventorySales.Application.DTOs.Category;
using InventorySales.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventorySales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CategoryCreateRequest request)
    {
        await _categoryService.CreateAsync(request);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(result);
    }

    //update
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CategoryUpdateRequest request)
    {
        try
        {
            await _categoryService.UpdateAsync(id, request);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    //delete
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _categoryService.DeleteAsync(id);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
