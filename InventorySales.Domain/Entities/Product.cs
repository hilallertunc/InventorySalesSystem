using InventorySales.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventorySales.Domain.Entities
{
    public class Product: BaseEntity
    {
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;  
    }
}