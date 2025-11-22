namespace ProductManagement.Presentation.DTOs.ProductInventory;

public class AddProductInventoryRequest
{
    public Guid ProductId { get; init; }
    public decimal Price { get; init; }
    public decimal Quantity { get; init; }
}