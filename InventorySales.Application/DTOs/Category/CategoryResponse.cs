using System;
using System.Collections.Generic;
using System.Text;

namespace InventorySales.Application.DTOs.Category
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }
}
