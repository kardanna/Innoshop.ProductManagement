namespace ProductManagement.Application.Contexts;

public record AddProductInventoryContext
{
    public Guid ProductId { get; init; }
    public decimal Price { get; init; }
    public decimal Quantity { get; init; }
    public Guid RequesterId { get; init; }

    public AddProductInventoryContext(
        Guid productId,
        decimal price,
        decimal quantity,
        Guid requesterId)
    {
        ProductId = productId;
        Price = price;
        Quantity = quantity;
        RequesterId = requesterId;
    }
}