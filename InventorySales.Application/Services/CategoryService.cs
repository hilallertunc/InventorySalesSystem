using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventorySales.Application.DTOs.Category;
using InventorySales.Domain.Entities;
using InventorySales.Infrastructure.Repositories;

namespace InventorySales.Application.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoryService(CategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task CreateAsync(CategoryCreateRequest request)
        {
            var category = new Category
            {
                Name = request.Name
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
    }
}
