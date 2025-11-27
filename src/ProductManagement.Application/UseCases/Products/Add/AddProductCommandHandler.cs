using ProductManagement.Application.Contexts;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Messaging;
using ProductManagement.Application.UseCases.Products.Get;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.UseCases.Products.Add;

public class AddProductCommandHandler : ICommandHandler<AddProductCommand, GetProductResponse>
{
    private readonly IProductOwnerService _ownerService;
    private readonly IProductService _productServcie;
    private readonly IUnitOfWork _unitOfWork;

    public AddProductCommandHandler(
        IProductOwnerService ownerService,
        IProductService productServcie,
        IUnitOfWork unitOfWork)
    {
        _ownerService = ownerService;
        _productServcie = productServcie;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GetProductResponse>> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        var owner = await _ownerService.GetOrCreateProductOwnerAsync(request.OwnerId);

        if (owner.IsFailure) return owner.Error;

        var context = new AddProductContext(
            owner: owner,
            name: request.Name,
            description: request.Description,
            measurementUnit: request.MeasurementUnit
        );

        var product = await _productServcie.AddAsync(context);

        if (product.IsFailure) return product.Error;

        await _unitOfWork.SaveChangesAsync();

        var response = new GetProductResponse(product);

        return response;
    }
}