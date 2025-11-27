using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Messaging;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.UseCases.Products.Delete;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly IProductService _productService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(
        IProductService productService,
        IUnitOfWork unitOfWork)
    {
        _productService = productService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var result = await _productService.DeleteAsync(request.ProductId, request.RequesterId);

        if (result.IsFailure) return result;

        await _unitOfWork.SaveChangesAsync();

        return result;
    }
}