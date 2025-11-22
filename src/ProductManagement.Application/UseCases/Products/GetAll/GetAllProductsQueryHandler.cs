using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Messaging;
using ProductManagement.Application.UseCases.Products.Get;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.UseCases.Products.GetAll;

public class GetAllProductsQueryHandler : IQueryHandler<GetAllProductsQuery, IEnumerable<GetProductResponse>>
{
    private readonly IProductService _productService;

    public GetAllProductsQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<IEnumerable<GetProductResponse>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(
            queryParameters: request.QueryParameters,
            requesterId: request.RequesterId
        );

        if (products.IsFailure) return products.Error;

        var response = products.Value.Select(p => new GetProductResponse(p));
        
        return response.ToList();
    }
}