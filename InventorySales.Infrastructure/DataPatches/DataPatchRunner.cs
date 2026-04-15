using InventorySales.Domain.Entities.System;
using InventorySales.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySales.Infrastructure.DataPatches
{
    public class DataPatchRunner
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataPatchRunner> _logger;
        private readonly IEnumerable<IDataPatch> _patches;

        public DataPatchRunner(
            IServiceProvider serviceProvider,
            ILogger<DataPatchRunner> logger,
            IEnumerable<IDataPatch> patches)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _patches = patches;
        }

        public async Task RunAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var appliedPatches = await context.DataPatchHistories
                .Select(x => x.PatchName)
                .ToListAsync();

            var pendingPatches = _patches
                .Where(p => !appliedPatches.Contains(p.PatchName))
                .OrderBy(p => p.PatchName)
                .ToList();

            if (pendingPatches.Count == 0)
            {
                _logger.LogInformation("No pending data patches found.");
                return;
            }

            _logger.LogInformation("{Count} pending data patch(es) found.", pendingPatches.Count);

            foreach (var patch in pendingPatches)
            {
                try
                {
                    _logger.LogInformation("Applying data patch: {PatchName}", patch.PatchName);

                    await patch.ApplyAsync(context);

                    context.DataPatchHistories.Add(new DataPatchHistory
                    {
                        PatchName = patch.PatchName,
                        AppliedAtUtc = DateTime.UtcNow,
                        AppliedBy = patch.AppliedBy
                    });

                    await context.SaveChangesAsync();

                    _logger.LogInformation("Data patch applied successfully: {PatchName}", patch.PatchName);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Failed to apply data patch: {PatchName}. Startup aborted.", patch.PatchName);
                    throw;
                }
            }
        }
    }
}