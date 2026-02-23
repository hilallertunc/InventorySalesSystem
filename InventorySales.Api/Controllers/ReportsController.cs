using InventorySales.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> DailySales([FromQuery] DateOnly date)
        {
            var result = await _reportService.GetDailySalesAsync(date);
            return Ok(result);
        }

        // top product
        [HttpGet("top-products")]
        public async Task<IActionResult> TopProducts(
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null,
            [FromQuery] int take = 10)
        {
            var result = await _reportService.GetTopSellingProductsAsync(from, to, take);
            return Ok(result);
        }

        // customer sum
        [HttpGet("customer-summary")]
        public async Task<IActionResult> CustomerSummary([FromQuery] string? userId = null)
        {
            var result = await _reportService.GetCustomerSummaryAsync(userId);
            return Ok(result);
        }

        // low stock 
        [HttpGet("low-stock")]
        public async Task<IActionResult> LowStock([FromQuery] int threshold = 5)
        {
            var result = await _reportService.GetLowStockAsync(threshold);
            return Ok(result);
        }

        // sales range
        [HttpGet("sales-range")]
        public async Task<IActionResult> SalesRange([FromQuery] DateOnly from, [FromQuery] DateOnly to)
        {
            var result = await _reportService.GetSalesRangeAsync(from, to);
            return Ok(result);
        }
    }
}
