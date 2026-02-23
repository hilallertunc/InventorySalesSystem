using InventorySales.Application.DTOs.Category;
using InventorySales.Application.DTOs.Common;
using InventorySales.Domain.Entities;
using InventorySales.Infrastructure.Repositories;

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


        public async Task CreateAsync(CategoryCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Category names cannot be empty.");

            var category = new Category
            {
                Name = request.Name.Trim()
            };

            await _categoryRepository.AddAsync(category);
        }

        public async Task<List<CategoryResponse>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            return categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
        }

        public async Task<PagedResult<CategoryResponse>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var (items, totalCount) = await _categoryRepository.GetPagedAsync(pageNumber, pageSize);

            return new PagedResult<CategoryResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items.Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList()
            };
        }
        public async Task UpdateAsync(int id,CategoryUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Category names cannot be empty.");
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category is null)
                throw new ArgumentException("Category not found.");
            category.Name = request.Name.Trim();
            await _categoryRepository.UpdateAsync(category);
        }
        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category is null)
                throw new ArgumentException("Category not found.");

            var hasProducts = await _productRepository.AnyByCategoryIdAsync(id);
            if (hasProducts)
                throw new ArgumentException("This category contains products. First, delete the products or move them to another category.");

            await _categoryRepository.DeleteAsync(category);
        }

    }
}
