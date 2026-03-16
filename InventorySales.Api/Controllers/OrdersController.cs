using InventorySales.Application.DTOs.Common;
using InventorySales.Application.DTOs.Order;
using InventorySales.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InventorySales.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public async Task<Result<int>> Create(OrderCreateRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Result<int>.Failure("Unauthorized user.");

            return await _orderService.CreateAsync(userId, request);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/cancel")]
        public async Task<Result> Cancel(int id)
        {
            return await _orderService.CancelAsync(id);
        }
    }
}