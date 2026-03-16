using InventorySales.Application.DTOs.Common;
using InventorySales.Application.DTOs.Product;
using InventorySales.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
        public async Task<Result<int>> Create(ProductCreateRequest request)
        {
            
            return await _productService.CreateAsync(request);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<Result<int>> Update(int id, ProductUpdateRequest request)
        {
            return await _productService.UpdateAsync(id, request);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<Result> Delete(int id)
        {
            return await _productService.DeleteAsync(id);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/restore")]
        public async Task<Result> Restore(int id)
        {
            return await _productService.RestoreAsync(id);
        }

        [HttpGet]
        public async Task<Result<PagedResult<ProductResponse>>> GetAll(
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
            if ((includeDeleted || onlyDeleted) && !User.IsInRole("Admin"))
            {
                
                return Result<PagedResult<ProductResponse>>.Failure("No access permission.");
            }

            var paging = new PagingRequest { PageNumber = pageNumber, PageSize = pageSize };

            return await _productService.GetPagedAsync(
                paging, search, categoryId, minPrice, maxPrice, sortBy, sortDir, includeDeleted, onlyDeleted);
        }
    }
}