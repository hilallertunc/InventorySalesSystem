using InventorySales.Application.DTOs.Common;
using InventorySales.Application.DTOs.Product;
using InventorySales.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventorySales.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateRequest request)
        {
            var result = await _productService.CreateAsync(request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductUpdateRequest request)
        {
            var result = await _productService.UpdateAsync(id, request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var result = await _productService.DeleteAsync(id, deletedBy: email);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        // sdmin
        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(int id)
        {
            var result = await _productService.RestoreAsync(id);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDir = null,
            [FromQuery] bool includeDeleted = false,
            [FromQuery] bool onlyDeleted = false)
        {
            var paging = new PagingRequest { PageNumber = pageNumber, PageSize = pageSize };

            var result = await _productService.GetPagedAsync(
                paging,
                search,
                categoryId,
                minPrice,
                maxPrice,
                sortBy,
                sortDir,
                includeDeleted,
                onlyDeleted);

            return Ok(result);
        }
    }
}