using System;
using System.Collections.Generic;
using System.Text;

namespace InventorySales.Application.DTOs.Order
{
    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderCreateRequest
    {
        public List<OrderItemRequest> Items { get; set; } = new();
    }
}
