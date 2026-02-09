using InventorySales.Application.DTOs.Category;
using InventorySales.Application.Services;
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
}
