using System.Threading.Tasks;

namespace InventorySales.Infrastructure.DataPatches
{
    public interface IDataPatch
    {
        string PatchName { get; }
        string AppliedBy { get; }
        Task ApplyAsync(Infrastructure.Data.AppDbContext context);
    }
}