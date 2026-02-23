using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventorySales.Application.DTOs.Common;
using InventorySales.Application.DTOs.Product;
using InventorySales.Domain.Entities;
using InventorySales.Infrastructure.Repositories;

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

    public async Task CreateAsync(ProductCreateRequest request)
    {
        // rule control
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Product names cannot be left blank.");

        if (request.Price <= 0)
            throw new ArgumentException("The price cannot be 0 or negative.");

        if (request.Stock < 0)
            throw new ArgumentException("The stock cannot be negative.");

        // category 
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
            throw new ArgumentException("Invalid CategoryId.");


        var product = new Product
        {
            Name = request.Name.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId
        };

        await _productRepository.AddAsync(product);
    }

    // update
    public async Task UpdateAsync(int id, ProductUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Product names cannot be left blank.");

        if (request.Price <= 0)
            throw new ArgumentException("The price cannot be 0 or negative.");

        if (request.Stock < 0)
            throw new ArgumentException("The stock cannot be negative.");

        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            throw new ArgumentException("Product not found.");

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null)
            throw new ArgumentException("Geçersiz CategoryId.");

        product.Name = request.Name.Trim();
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;

        await _productRepository.UpdateAsync(product);
    }

    // delete
    public async Task DeleteAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            throw new ArgumentException("Product not found.");

        await _productRepository.DeleteAsync(product);
    }

    // paginated listing
    public async Task<PagedResult<ProductResponse>> GetPagedAsync(
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
        var (items, totalCount) = await _productRepository.GetPagedWithCategoryAsync(pageNumber, pageSize);
        return new PagedResult<ProductResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items.Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? ""
            }).ToList()

        };
    }

    public async Task<List<ProductResponse>> GetAllAsync()
    {
        var products = await _productRepository.GetAllWithCategoryAsync();

        return products.Select(p => new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Stock = p.Stock,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? ""
        }).ToList();
    }




}
