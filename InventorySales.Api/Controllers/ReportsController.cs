using InventorySales.Application.DTOs.Common;
using InventorySales.Application.DTOs.Reports;
using InventorySales.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventorySales.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        // daily sales
        [HttpGet("daily-sales")]
        public async Task<Result<DailySalesReportResponse>> DailySales([FromQuery] DateOnly date)
        {
            return await _reportService.GetDailySalesAsync(date);
        }

        // top product
        [HttpGet("top-products")]
        public async Task<Result<List<TopSellingProductResponse>>> TopProducts(
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null,
            [FromQuery] int take = 10)
        {
            return await _reportService.GetTopSellingProductsAsync(from, to, take);
        }

        // customer sum
        [HttpGet("customer-summary")]
        public async Task<Result<List<CustomerOrderSummaryResponse>>> CustomerSummary([FromQuery] string? userId = null)
        {
            return await _reportService.GetCustomerSummaryAsync(userId);
        }

        // low stock 
        [HttpGet("low-stock")]
        public async Task<Result<List<LowStockProductResponse>>> LowStock([FromQuery] int threshold = 5)
        {
            return await _reportService.GetLowStockAsync(threshold);
        }

        // sales range
        [HttpGet("sales-range")]
        public async Task<Result<SalesRangeReportResponse>> SalesRange([FromQuery] DateOnly from, [FromQuery] DateOnly to)
        {
            return await _reportService.GetSalesRangeAsync(from, to);
        }
    }
}