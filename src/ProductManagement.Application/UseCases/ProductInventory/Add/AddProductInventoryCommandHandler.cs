using ProductManagement.Application.Contexts;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Messaging;
using ProductManagement.Application.UseCases.Products.Get;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.UseCases.ProductInventory.Add;

public class AddProductInventoryCommandHandler : ICommandHandler<AddProductInventoryCommand, GetProductResponse>
{
    private readonly IProductService _productService;

    public AddProductInventoryCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result<GetProductResponse>> Handle(AddProductInventoryCommand request, CancellationToken cancellationToken)
    {
        var context = new AddProductInventoryContext(
            productId: request.ProductId,
            price: request.Price,
            quantity: request.Quantity,
            requesterId: request.RequseterId
        );

        var product = await _productService.AddInventoryRecordAsync(context);

        if (product.IsFailure) return product.Error;

        var response = new GetProductResponse(product);
        
        return response;
    }
}