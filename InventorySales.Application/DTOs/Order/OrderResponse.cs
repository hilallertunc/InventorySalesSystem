using System;
using System.Collections.Generic;
using System.Text;

namespace InventorySales.Application.DTOs.Order
{
    public class OrderItemResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderResponse
    {
        public int Id { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; }
        public decimal Total { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
    }
}
