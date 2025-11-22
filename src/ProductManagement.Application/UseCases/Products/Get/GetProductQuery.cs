using ProductManagement.Application.Messaging;

namespace ProductManagement.Application.UseCases.Products.Get;

public class GetProductQuery : IQuery<GetProductResponse>
{
    public Guid ProductId { get; init; }
    public Guid? RequesterId { get; init; }

    public GetProductQuery(
        Guid productId,
        Guid? requesterId = null)
    {
        ProductId = productId;
        RequesterId = requesterId;
    }
}