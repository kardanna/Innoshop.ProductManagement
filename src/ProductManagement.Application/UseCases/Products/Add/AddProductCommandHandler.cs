using ProductManagement.Application.Contexts;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Messaging;
using ProductManagement.Application.UseCases.Products.Get;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.UseCases.Products.Add;

public class AddProductCommandHandler : ICommandHandler<AddProductCommand, GetProductResponse>
{
    private readonly IProductOwnerService _ownerService;
    private readonly IProductService _productServcie;

    public AddProductCommandHandler(
        IProductOwnerService ownerService,
        IProductService productServcie)
    {
        _ownerService = ownerService;
        _productServcie = productServcie;
    }

    public async Task<Result<GetProductResponse>> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        var owner = await _ownerService.GetOrCreateProductOwnerAsync(request.OwnerId);

        if (owner.IsFailure) return owner.Error;

        var unit = MeasurementUnit.GetByName(request.MeasurementUnit);

        if (unit is null) return DomainErrors.MeasurementUnit.UnknownUnit;

        var context = new AddProductContext(
            owner: owner,
            name: request.Name,
            description: request.Description,
            measurementUnit: unit
        );

        var product = await _productServcie.AddAsync(context);

        if (product.IsFailure) return product.Error;

        var response = new GetProductResponse(product);

        return response;
    }
}