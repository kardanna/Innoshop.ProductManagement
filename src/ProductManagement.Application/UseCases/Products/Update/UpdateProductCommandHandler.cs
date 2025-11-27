using ProductManagement.Application.Contexts;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Messaging;
using ProductManagement.Application.UseCases.Products.Get;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.UseCases.Products.Update;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, GetProductResponse>
{
    private readonly IProductService _productServcie;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(
        IProductService productServcie,
        IUnitOfWork unitOfWork)
    {
        _productServcie = productServcie;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GetProductResponse>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var context = new UpdateProductContext(
            productId: request.ProductId,
            name: request.Name,
            description: request.Description,
            measurementUnit: request.MeasurementUnit,
            requesterId: request.RequesterId
        );

        var product = await _productServcie.UpdateAsync(context);

        if (product.IsFailure) return product.Error;

        await _unitOfWork.SaveChangesAsync();

        var response = new GetProductResponse(product);

        return response;
    }
}