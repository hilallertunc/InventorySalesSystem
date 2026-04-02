using InventorySales.Application.DTOs.Auth;
using InventorySales.Application.Interfaces;
using InventorySales.Infrastructure.Services; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySales.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
       
        private readonly IAuthService _authService;
        private readonly ICacheService _cacheService;

        public AuthController(IAuthService authService, ICacheService cacheService)
        {
            _authService = authService;
            _cacheService = cacheService;
        }

        public record RegisterRequest(string Email, string Password);
       

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request.Email, request.Password);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            
            var result = await _authService.LoginAsync(request);

            return result.IsSuccess ? Ok(result) : Unauthorized(result);
        }

        
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (Request.Headers.TryGetValue("Authorization", out StringValues authHeader))
            {
                var token = authHeader.FirstOrDefault()?.Split(" ").Last();

                if (!string.IsNullOrEmpty(token))
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);

                    var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

                    if (expClaim != null)
                    {
                        var expDateTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;
                        var timeRemaining = expDateTime - DateTime.UtcNow;

                        if (timeRemaining > TimeSpan.Zero)
                        {
                            string cacheKey = $"Blacklist_{token}";
                            await _cacheService.SetAsync(cacheKey, "revoked", timeRemaining);
                        }
                    }
                }
            }

            return Ok(new { isSuccess = true, message = "Logged out successfully." });
        }
    }
}