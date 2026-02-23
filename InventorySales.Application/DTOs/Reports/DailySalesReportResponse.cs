using System;
using System.Collections.Generic;
using System.Text;

namespace InventorySales.Application.DTOs.Reports
{
    public class DailySalesReportResponse
    {
        public DateOnly Date {  get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSales { get; set; }
    }
}
