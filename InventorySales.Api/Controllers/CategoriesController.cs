using InventorySales.Application.DTOs.Category;
using InventorySales.Application.DTOs.Common;
using InventorySales.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventorySales.Api.Controllers
{
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
        public async Task<Result<int>> Create(CategoryCreateRequest request)
        {
            return await _categoryService.CreateAsync(request);
        }

        [HttpGet]
        public async Task<Result<List<CategoryResponse>>> GetAll()
        {
            return await _categoryService.GetAllAsync();
        }

        [HttpGet("paged")]
        public async Task<Result<PagedResult<CategoryResponse>>> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var pagingRequest = new PagingRequest { PageNumber = pageNumber, PageSize = pageSize };
            return await _categoryService.GetPagedAsync(pagingRequest);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<Result<int>> Update(int id, CategoryUpdateRequest request)
        {
            return await _categoryService.UpdateAsync(id, request);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<Result> Delete(int id)
        {
            return await _categoryService.DeleteAsync(id);
        }
    }
}