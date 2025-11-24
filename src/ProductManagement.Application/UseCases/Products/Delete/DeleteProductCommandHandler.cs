using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Messaging;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.UseCases.Products.Delete;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly IProductService _productService;

    public DeleteProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var response = await _productService.DeleteAsync(request.ProductId, request.RequesterId);

        return response;
    }
}