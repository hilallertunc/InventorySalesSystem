using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventorySales.Application.DTOs.Common;
using InventorySales.Application.DTOs.Product;
using InventorySales.Application.Extensions;
using InventorySales.Domain.Entities;
using InventorySales.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InventorySales.Application.Services;

public class ProductService
{
    private readonly ProductRepository _productRepository;
    private readonly CategoryRepository _categoryRepository;

    public ProductService(ProductRepository productRepository, CategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result> CreateAsync(ProductCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure("Product names cannot be left blank.");

        if (request.Price <= 0)
            return Result.Failure("The price cannot be 0 or negative.");

        if (request.Stock < 0)
            return Result.Failure("The stock cannot be negative.");

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
            return Result.Failure("Invalid CategoryId.");

        var product = new Product
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId
        };

        await _productRepository.AddAsync(product);
        return Result.Success("Product created successfully.");
    }

    public async Task<Result> UpdateAsync(int id, ProductUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure("Product names cannot be left blank.");

        if (request.Price <= 0)
            return Result.Failure("The price cannot be 0 or negative.");

        if (request.Stock < 0)
            return Result.Failure("The stock cannot be negative.");

        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            return Result.Failure("Product not found.");

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null)
            return Result.Failure("Invalid CategoryId.");

        product.Name = request.Name.Trim();
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;

        await _productRepository.UpdateAsync(product);
        return Result.Success("Product updated successfully.");
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            return Result.Failure("Product not found.");

        await _productRepository.DeleteAsync(product);
        return Result.Success("Product deleted successfully.");
    }

    public async Task<Result> RestoreAsync(int id)
    {
        
        var query = _productRepository.GetProductsWithCategoryQuery().IgnoreQueryFilters();
        var product = await query.FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
            return Result.Failure("Product not found.");

        product.IsDeleted = false;
        await _productRepository.UpdateAsync(product);
        return Result.Success("Product restored successfully.");
    }

    public async Task<Result<PagedResult<ProductResponse>>> GetPagedAsync(
        PagingRequest pagingRequest,
        string? search,
        int? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? sortBy,
        string? sortDir,
        bool includeDeleted = false,
        bool onlyDeleted = false)
    {
        var query = _productRepository.GetProductsWithCategoryQuery();

        
        if (includeDeleted || onlyDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

       
        if (onlyDeleted)
        {
            query = query.Where(p => p.IsDeleted == true);
        }

        // search and filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        // sort
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        query = (sortBy?.ToLower()) switch
        {
            "price" => desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "name" => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "stock" => desc ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock),
            _ => query.OrderBy(p => p.Id)
        };

        
        var selectQuery = query.Select(p => new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Stock = p.Stock,
            CategoryId = p.CategoryId,
            CategoryName = p.Category != null ? p.Category.Name : ""
        });

        // paging
        var pagedData = await selectQuery.ToPagedResultAsync(pagingRequest);

        return Result<PagedResult<ProductResponse>>.Success(pagedData);
    }
}