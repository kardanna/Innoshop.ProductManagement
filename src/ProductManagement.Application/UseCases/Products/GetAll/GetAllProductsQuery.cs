using ProductManagement.Application.Messaging;
using ProductManagement.Application.UseCases.Products.Get;

namespace ProductManagement.Application.UseCases.Products.GetAll;

public class GetAllProductsQuery : IQuery<IEnumerable<GetProductResponse>>
{
    public GetAllProductsQueryParameters? QueryParameters { get; init; }
    public Guid? RequesterId { get; init; }

    public GetAllProductsQuery(
        GetAllProductsQueryParameters? queryParameters = null,
        Guid? requesterId = null)
    {
        QueryParameters = queryParameters;
        RequesterId = requesterId;
    }
}