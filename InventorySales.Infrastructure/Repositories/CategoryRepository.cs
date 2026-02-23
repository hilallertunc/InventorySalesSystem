using InventorySales.Domain.Entities;
using InventorySales.Infrastructure.Data;
using InventorySales.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace InventorySales.Infrastructure.Repositories
{
    public class CategoryRepository : BaseRepository<Category>
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }

        
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }
    }
}
