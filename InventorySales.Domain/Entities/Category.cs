using System;
using System.Collections.Generic;
using System.Text;

namespace InventorySales.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!; //gecici null 
    }
}
