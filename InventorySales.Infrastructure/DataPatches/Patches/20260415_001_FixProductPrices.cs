using InventorySales.Infrastructure.Data;
using InventorySales.Infrastructure.DataPatches;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace InventorySales.Infrastructure.DataPatches.Patches
{
    public class _20260415_001_FixProductPrices : IDataPatch
    {
        public string PatchName => "20260415_001_FixProductPrices";
        public string AppliedBy => "Hilal Ertunc";

        public async Task ApplyAsync(AppDbContext context)
        {
            var products = await context.Products
                .Where(p => p.Price <= 0)
                .ToListAsync();

            foreach (var product in products)
            {
                product.Price = 1;
            }

            await context.SaveChangesAsync();
        }
    }
}