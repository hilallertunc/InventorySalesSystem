using InventorySales.Infrastructure.Data;
using InventorySales.Infrastructure.DataPatches;
using System.Threading.Tasks;

namespace InventorySales.Infrastructure.DataPatches.Patches
{
    public class _20260420_002_TestPatch : IDataPatch
    {
        public string PatchName => "20260420_002_TestPatch";
        public string AppliedBy => "Test User";

        public async Task ApplyAsync(AppDbContext context)
        {
            await Task.CompletedTask;
        }
    }
}