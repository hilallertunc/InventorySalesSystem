using InventorySales.Domain.Entities;
using InventorySales.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using InventorySales.Infrastructure.Repositories.Base;

namespace InventorySales.Infrastructure.Repositories;

public class ProductRepository : BaseRepository<Product>
{

    public ProductRepository(AppDbContext context) : base(context)
    {
    }
    // product list
    public async Task<List<Product>> GetAllWithCategoryAsync()
    {
        return await _context.Products.Include( p=> p.Category).ToListAsync();
    }
    //product registration
    public async Task<Product?> GetByIdWithCategoryAsync(int id)
    {
        return await _context.Products.Include(p=> p.Category).FirstOrDefaultAsync(p=> p.Id == id);
    }
    // paging
    public async Task<(List<Product> Items, int TotalCount)> GetPagedWithCategoryAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.Products.Include(p => p.Category);
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }
    public async Task<bool> AnyByCategoryIdAsync(int categoryId)
    {
        return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
    }

    public async Task<(List<Product> Items, int TotalCount)> GetPagedFilteredWithCategoryAsync(
        int pageNumber,
        int pageSize,
        string? search,
        int? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? sortBy,
        string? sortDir
        )
    {
        if (pageNumber < 1)
            pageNumber = 1;
        if(pageSize < 1)
            pageSize = 10;
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        // name search
        if(!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }

        // category filter
        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        // price range filter
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = (sortBy?.ToLower()) switch
        {
            "price" => desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "name" => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "stock" => desc ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock),
            _ => query.OrderBy(p => p.Id)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);

    }
}
