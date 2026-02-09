namespace InventorySales.Application.DTOs.Product;

public class ProductCreateRequest
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
}
