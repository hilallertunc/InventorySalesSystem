using InventorySales.Domain.Entities.Common;
using System.Collections.Generic;

namespace InventorySales.Domain.Entities.Orders
{
    public class Order : BaseEntity
    {
        public string UserId { get; set; } = default!;
        public OrderStatus Status { get; set; } = OrderStatus.Created;
        public List<OrderItem> Items { get; set; } = new();
    }
}