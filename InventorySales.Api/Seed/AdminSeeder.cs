using InventorySales.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace InventorySales.Api.Seed
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            var adminEmail = "admin@inventory.com";
            var adminPassword = "Admin123!";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail
                };

                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (!createResult.Succeeded)
                    return; 
            }

            // admin role
            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
