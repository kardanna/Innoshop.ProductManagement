using ProductManagement.Application.Contexts;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Messaging;
using ProductManagement.Application.UseCases.Products.Get;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.UseCases.Products.Update;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, GetProductResponse>
{
    private readonly IProductOwnerService _ownerService;
    private readonly IProductService _productServcie;

    public UpdateProductCommandHandler(
        IProductOwnerService ownerService,
        IProductService productServcie)
    {
        _ownerService = ownerService;
        _productServcie = productServcie;
    }

    public async Task<Result<GetProductResponse>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var unit = MeasurementUnit.GetByName(request.MeasurementUnit);

        if (unit is null) return DomainErrors.MeasurementUnit.UnknownUnit;

        var context = new UpdateProductContext(
            productId: request.ProductId,
            name: request.Name,
            description: request.Description,
            measurementUnit: unit,
            requesterId: request.RequesterId
        );

        var product = await _productServcie.UpdateAsync(context);

        if (product.IsFailure) return product.Error;

        var response = new GetProductResponse(product);

        return response;
    }
}