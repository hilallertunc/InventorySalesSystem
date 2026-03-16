using System;
using System.Collections.Generic;
using System.Text;

namespace InventorySales.Domain.Entities.System
{
    public class LogEntry
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public  string? StackTrace { get; set; }
        public string? Path { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
