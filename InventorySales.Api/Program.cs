using InventorySales.Api.Extensions;
using InventorySales.Application.Interfaces;
using InventorySales.Application.Services;
using InventorySales.Domain.Entities.Identity;
using InventorySales.Infrastructure.Data;
using InventorySales.Infrastructure.DataPatches;
using InventorySales.Infrastructure.DataPatches.Patches;
using InventorySales.Infrastructure.Repositories;
using InventorySales.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null)
    ));

builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetSection("RedisCacheSettings:ConnectionString").Value;
    options.InstanceName = builder.Configuration.GetSection("RedisCacheSettings:InstanceName").Value;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwt = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IDataPatch, _20260415_001_FixProductPrices>();
builder.Services.AddScoped<IDataPatch, _20260420_002_TestPatch>();
builder.Services.AddScoped<DataPatchRunner>();

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();
await app.Services.ApplyDataPatchesAsync();

await InventorySales.Api.Seed.RoleSeeder.SeedAsync(app.Services);
await InventorySales.Api.Seed.AdminSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<InventorySales.Api.Middlewares.ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<InventorySales.Api.Middlewares.TokenBlacklistMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();