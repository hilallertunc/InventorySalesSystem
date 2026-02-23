using System;
using System.Collections.Generic;
using System.Text;

namespace InventorySales.Application.DTOs.Reports
{
    public class LowStockProductResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty ;
    }
}
