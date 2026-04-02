using InventorySales.Application.DTOs.Auth;
using InventorySales.Application.DTOs.Common;
using InventorySales.Application.Interfaces;
using InventorySales.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace InventorySales.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;

        public AuthService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IDistributedCache cache)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _cache = cache;
        }

        public async Task<Result> RegisterAsync(string email, string password)
        {
            var userExists = await _userManager.FindByEmailAsync(email);
            if (userExists != null)
                return Result.Failure("User already exists!");

            AppUser user = new()
            {
                Email = email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = email
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return Result.Failure("User creation failed!", result.Errors.Select(e => e.Description).ToList());

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            await _userManager.AddToRoleAsync(user, "User");

            return Result.Success("User created successfully!");
        }

        public async Task<Result<string>> LoginAsync(LoginDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Result<string>.Failure("Invalid username or password");
            }

            var cacheKey = $"active_token_{user.Id}";
            var existingToken = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(existingToken))
            {
                if (!request.ForceRelogin)
                {
                    return Result<string>.Failure("There is already an active session for this user. Please use 'ForceRelogin' to log out from other devices.");
                }
                else
                {
                    await _cache.RemoveAsync(cacheKey);
                }
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GetToken(authClaims);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3)
            };
            await _cache.SetStringAsync(cacheKey, tokenString, cacheOptions);

            return Result<string>.Success(tokenString, "Login successful");
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }
    }
}