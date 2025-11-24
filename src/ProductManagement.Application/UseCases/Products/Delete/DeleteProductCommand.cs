using ProductManagement.Application.Messaging;

namespace ProductManagement.Application.UseCases.Products.Delete;

public class DeleteProductCommand : ICommand
{
    public Guid ProductId { get; init; }
    public Guid RequesterId { get; init; }

    public DeleteProductCommand(
        Guid productId,
        Guid requesterId
    )
    {
        ProductId = productId;
        RequesterId = requesterId;
    }
}