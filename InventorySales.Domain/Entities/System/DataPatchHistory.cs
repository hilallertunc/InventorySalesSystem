using System;

namespace InventorySales.Domain.Entities.System
{
    public class DataPatchHistory
    {
        public int Id { get; set; }
        public string PatchName { get; set; } = default!;
        public DateTime AppliedAtUtc { get; set; }
        public string AppliedBy { get; set; } = default!;
    }
}