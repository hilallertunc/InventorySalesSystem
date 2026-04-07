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
        private readonly IMailService _mailService;

        public AuthService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IDistributedCache cache,
            IMailService mailService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _cache = cache;
            _mailService = mailService;
        }

        public async Task<Result> RegisterAsync(string email, string password)
        {
            var userExists = await _userManager.FindByEmailAsync(email);
            if (userExists != null) return Result.Failure("User already exists!");

            AppUser user = new() { Email = email, SecurityStamp = Guid.NewGuid().ToString(), UserName = email };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) return Result.Failure("User creation failed!", result.Errors.Select(e => e.Description).ToList());

            if (!await _roleManager.RoleExistsAsync("User")) await _roleManager.CreateAsync(new IdentityRole("User"));
            await _userManager.AddToRoleAsync(user, "User");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"http://localhost:8080/api/auth/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            await _mailService.SendEmailAsync(user.Email!, "Email Confirmation",
                $"Please click <a href='{confirmationLink}'>here to confirm your account.</a>");

            return Result.Success("Registration successful. Please click the confirmation link sent to your email address.");
        }

        public async Task<Result<string>> LoginAsync(LoginDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password)) return Result<string>.Failure("Invalid username or password");
            if (!user.EmailConfirmed) return Result<string>.Failure("Email not verified. Please check your mailbox.");

            var cacheKey = $"active_token_{user.Id}";
            var existingToken = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(existingToken) && !request.ForceRelogin)
            {
                return Result<string>.Failure("User is already active on another session.");
            }
            var tokenString = new JwtSecurityTokenHandler().WriteToken(GetToken(new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim(ClaimTypes.Email, user.Email!) }));
            await _cache.SetStringAsync(cacheKey, tokenString, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3) });

            return Result<string>.Success(tokenString, "Login successful");
        }

        public async Task<Result> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Result.Failure("User not found.");
            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded ? Result.Success("Email confirmed successfully!") : Result.Failure("Email confirmation failed.");
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "superSecretKey123456789"));
            return new JwtSecurityToken(issuer: _configuration["Jwt:Issuer"], audience: _configuration["Jwt:Audience"], expires: DateTime.Now.AddHours(3), claims: authClaims, signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));
        }
    }
}