using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Messaging;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.UseCases.Products.Get;

public class GetProductQueryHandler : IQueryHandler<GetProductQuery, GetProductResponse>
{
    private readonly IProductService _productService;

    public GetProductQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<GetProductResponse>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _productService.GetAsync(
            productId: request.ProductId,
            requesterId: request.RequesterId
        );

        if (product.IsFailure) return product.Error;

        var response = new GetProductResponse(product);

        return response;
    }
}