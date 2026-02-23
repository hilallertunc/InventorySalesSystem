using InventorySales.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventorySales.Domain.Entities.Orders
{
    public class Order : BaseEntity
    {
        public string UserId { get; set; } = default!;
        public OrderStatus Status { get; set; } = OrderStatus.Created;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public List<OrderItem> Items { get; set; } = new();
    }
}
