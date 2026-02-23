using System;
using System.Collections.Generic;
using System.Text;

namespace InventorySales.Application.DTOs.Reports
{
    public class SalesRangeReportResponse
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSales { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
