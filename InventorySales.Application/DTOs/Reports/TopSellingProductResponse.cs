using System;
using System.Collections.Generic;
using System.Text;

namespace InventorySales.Application.DTOs.Reports
{
    public class TopSellingProductResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
