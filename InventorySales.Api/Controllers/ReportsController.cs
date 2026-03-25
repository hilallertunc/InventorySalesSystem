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

        [HttpGet("daily-sales")]
        public async Task<Result<DailySalesReportResponse>> GetDailySales([FromQuery] DateOnly date)
        {
            return await _reportService.GetDailySalesAsync(date);
        }

        [HttpGet("top-selling")]
        public async Task<Result<List<TopSellingProductResponse>>> GetTopSelling([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] int take = 10)
        {
            return await _reportService.GetTopSellingProductsAsync(from, to, take);
        }

        [HttpGet("customer-summary")]
        public async Task<Result<List<CustomerOrderSummaryResponse>>> GetCustomerSummary([FromQuery] string? userId)
        {
            return await _reportService.GetCustomerSummaryAsync(userId);
        }

        [HttpGet("low-stock")]
        public async Task<Result<List<LowStockProductResponse>>> GetLowStock([FromQuery] int threshold = 10)
        {
            return await _reportService.GetLowStockAsync(threshold);
        }

        [HttpGet("sales-range")]
        public async Task<Result<SalesRangeReportResponse>> GetSalesRange([FromQuery] DateOnly from, [FromQuery] DateOnly to)
        {
            return await _reportService.GetSalesRangeAsync(from, to);
        }
    }
}