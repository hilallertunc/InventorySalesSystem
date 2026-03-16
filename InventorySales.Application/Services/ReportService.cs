using InventorySales.Application.DTOs.Common;
using InventorySales.Application.DTOs.Reports;
using InventorySales.Domain.Entities.Orders;
using InventorySales.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySales.Application.Services
{
    public class ReportService
    {
        private readonly AppDbContext _context;
        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        // daily sales
        public async Task<Result<DailySalesReportResponse>> GetDailySalesAsync(DateOnly date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var end = start.AddDays(1);

            var ordersQuery = _context.Orders
                .AsNoTracking()
                .Where(o => o.CreatedAtUtc >= start && o.CreatedAtUtc < end)
                .Where(o => o.Status != OrderStatus.Cancelled);

            var orderCount = await ordersQuery.CountAsync();
            var totalSales = await ordersQuery
               .SelectMany(o => o.Items)
               .SumAsync(i => i.UnitPrice * i.Quantity);

            var data = new DailySalesReportResponse
            {
                Date = date,
                OrderCount = orderCount,
                TotalSales = totalSales
            };

            return Result<DailySalesReportResponse>.Success(data);
        }

        // best selling products
        public async Task<Result<List<TopSellingProductResponse>>> GetTopSellingProductsAsync(
            DateOnly? from,
            DateOnly? to,
            int take = 10)
        {
            if (take < 1) take = 10;

            var query = _context.OrderItems
                .AsNoTracking()
                .Include(i => i.Product)
                .Include(i => i.Order)
                .Where(i => i.Order != null && i.Order.Status != OrderStatus.Cancelled)
                .AsQueryable();

            if (from.HasValue)
            {
                var start = from.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                query = query.Where(i => i.Order!.CreatedAtUtc >= start);
            }

            if (to.HasValue)
            {
                var endExclusive = to.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);
                query = query.Where(i => i.Order!.CreatedAtUtc < endExclusive);
            }

            var result = await query
                .GroupBy(i => new { i.ProductId, Name = i.Product!.Name })
                .Select(g => new TopSellingProductResponse
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.UnitPrice * x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ThenByDescending(x => x.TotalRevenue)
                .Take(take)
                .ToListAsync();

            return Result<List<TopSellingProductResponse>>.Success(result);
        }

        // customer based order summary
        public async Task<Result<List<CustomerOrderSummaryResponse>>> GetCustomerSummaryAsync(string? userId)
        {
            var orders = _context.Orders
                .AsNoTracking()
                .Where(o => o.Status != OrderStatus.Cancelled)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(userId))
                orders = orders.Where(o => o.UserId == userId);

            var summary = await orders
                .Select(o => new
                {
                    o.UserId,
                    Total = o.Items.Sum(i => i.UnitPrice * i.Quantity)
                })
                .GroupBy(x => x.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    OrderCount = g.Count(),
                    TotalSpend = g.Sum(x => x.Total)
                })
                .ToListAsync();

            var userIds = summary.Select(s => s.UserId).ToList();

            var users = await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();

            var data = summary
                .Select(s =>
                {
                    var u = users.FirstOrDefault(x => x.Id == s.UserId);
                    return new CustomerOrderSummaryResponse
                    {
                        UserId = s.UserId,
                        Email = u?.Email ?? "",
                        OrderCount = s.OrderCount,
                        TotalSpend = s.TotalSpend
                    };
                })
                .OrderByDescending(x => x.TotalSpend)
                .ToList();

            return Result<List<CustomerOrderSummaryResponse>>.Success(data);
        }

        // low stock
        public async Task<Result<List<LowStockProductResponse>>> GetLowStockAsync(int threshold)
        {
            if (threshold < 0) threshold = 0;

            var result = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.Stock < threshold)
                .OrderBy(p => p.Stock)
                .Select(p => new LowStockProductResponse
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : ""
                })
                .ToListAsync();

            return Result<List<LowStockProductResponse>>.Success(result);
        }

        // sales report for date range
        public async Task<Result<SalesRangeReportResponse>> GetSalesRangeAsync(DateOnly from, DateOnly to)
        {
            var start = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endExclusive = to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1);

            var ordersQuery = _context.Orders
                .AsNoTracking()
                .Where(o => o.CreatedAtUtc >= start && o.CreatedAtUtc < endExclusive)
                .Where(o => o.Status != OrderStatus.Cancelled);

            var orderCount = await ordersQuery.CountAsync();

            var totalSales = await ordersQuery
                .SelectMany(o => o.Items)
                .SumAsync(i => i.UnitPrice * i.Quantity);

            var average = orderCount == 0 ? 0 : totalSales / orderCount;

            var data = new SalesRangeReportResponse
            {
                From = from,
                To = to,
                OrderCount = orderCount,
                TotalSales = totalSales,
                AverageOrderValue = average
            };

            return Result<SalesRangeReportResponse>.Success(data);
        }
    }
}