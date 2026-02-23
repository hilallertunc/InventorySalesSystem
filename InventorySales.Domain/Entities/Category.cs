using System;
using System.Collections.Generic;
using System.Text;
using InventorySales.Domain.Entities.Common;
namespace InventorySales.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = default!; //gecici null 
    }
}
