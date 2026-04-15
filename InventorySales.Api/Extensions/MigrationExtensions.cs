using InventorySales.Infrastructure.Data;
using InventorySales.Infrastructure.DataPatches;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySales.Api.Extensions
{
    public static class MigrationExtensions
    {
        public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                var pendingMigrations = (await db.Database.GetPendingMigrationsAsync()).ToList();

                if (pendingMigrations.Count == 0)
                {
                    logger.LogInformation("No pending schema migrations found. Database is up to date.");
                }
                else
                {
                    logger.LogInformation("{Count} pending schema migration(s): {Migrations}",
                        pendingMigrations.Count,
                        string.Join(", ", pendingMigrations));

                    await db.Database.MigrateAsync();

                    logger.LogInformation("All schema migrations applied successfully.");
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Schema migration failed. Application startup aborted.");
                throw;
            }
        }

        public static async Task ApplyDataPatchesAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<DataPatchRunner>();
            await runner.RunAsync();
        }
    }
}