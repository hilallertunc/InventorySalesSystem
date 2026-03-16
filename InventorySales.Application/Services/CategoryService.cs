using InventorySales.Application.DTOs.Category;
using InventorySales.Application.DTOs.Common;
using InventorySales.Application.Extensions;
using InventorySales.Domain.Entities;
using InventorySales.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySales.Application.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly ProductRepository _productRepository;

        public CategoryService(CategoryRepository categoryRepository, ProductRepository productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
        }

        
        public async Task<Result<int>> CreateAsync(CategoryCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<int>.Failure("Category names cannot be empty.");

            var category = new Category { Name = request.Name.Trim() };
            await _categoryRepository.AddAsync(category);

            return Result<int>.Success(category.Id, "Category created successfully.");
        }

        public async Task<Result<List<CategoryResponse>>> GetAllAsync()
        {
            var list = await _categoryRepository.GetQueryable()
                .Select(c => new CategoryResponse { Id = c.Id, Name = c.Name })
                .ToListAsync();

            return Result<List<CategoryResponse>>.Success(list);
        }

        public async Task<Result<PagedResult<CategoryResponse>>> GetPagedAsync(PagingRequest request)
        {
            var query = _categoryRepository.GetQueryable()
                .Select(c => new CategoryResponse { Id = c.Id, Name = c.Name });

            var paged = await query.ToPagedResultAsync(request);
            return Result<PagedResult<CategoryResponse>>.Success(paged);
        }

        
        public async Task<Result<int>> UpdateAsync(int id, CategoryUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<int>.Failure("Category names cannot be empty.");

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category is null)
                return Result<int>.Failure("Category not found.");

            category.Name = request.Name.Trim();
            await _categoryRepository.UpdateAsync(category);

            return Result<int>.Success(category.Id, "Category updated successfully.");
        }

        public async Task<Result> DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category is null)
                return Result.Failure("Category not found.");

            var hasProducts = await _productRepository.AnyByCategoryIdAsync(id);
            if (hasProducts)
                return Result.Failure("This category contains products. First, delete the products or move them.");

            await _categoryRepository.DeleteAsync(category);
            return Result.Success("Category deleted successfully.");
        }
    }
}