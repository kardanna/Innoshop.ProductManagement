using ProductManagement.Application.Messaging;
using ProductManagement.Application.UseCases.Products.Get;

namespace ProductManagement.Application.UseCases.ProductInventory.Add;

public class AddProductInventoryCommand : ICommand<GetProductResponse>
{
    public Guid ProductId { get; init; }
    public decimal Price { get; init; }
    public decimal Quantity { get; init; }
    public Guid RequseterId { get; init; }

    public AddProductInventoryCommand(
        Guid productId,
        decimal price,
        decimal quantity,
        Guid requseterId)
    {
        ProductId = productId;
        Price = price;
        Quantity = quantity;
        RequseterId = requseterId;
    }
}