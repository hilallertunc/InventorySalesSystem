using System;
using System.Collections.Generic;
using System.Text;

namespace InventorySales.Application.DTOs.Reports
{
    public class CustomerOrderSummaryResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalSpend { get; set; }
    }
}
