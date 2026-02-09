namespace InventorySales.Application.DTOs.Product;

public class ProductUpdateRequest
{
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
}
