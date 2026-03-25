using InventorySales.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySales.Api.Middlewares
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenBlacklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICacheService cacheService)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var cacheKey = $"Blacklist_{token}";

                var isBlacklisted = await cacheService.GetAsync<string>(cacheKey);

                if (!string.IsNullOrEmpty(isBlacklisted))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"isSuccess\": false, \"message\": \"Token has been revoked. Please login again.\"}");
                    return;
                }
            }

            await _next(context);
        }
    }
}