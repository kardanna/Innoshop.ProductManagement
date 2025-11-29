using ProductManagement.Application.Contexts;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Repositories;
using ProductManagement.Application.UseCases.Products.GetAll;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductPolicy _productPolicy;

    public ProductService(
        IProductRepository productRepository,
        IProductPolicy productPolicy)
    {
        _productRepository = productRepository;
        _productPolicy = productPolicy;
    }

    public async Task<Result<Product>> AddAsync(AddProductContext context)
    {
        var attempt = await _productPolicy.IsCreationAllowedAsync(context);

        if (attempt.IsDenied) return attempt.Error;

        var unit = MeasurementUnit.GetByName(context.MeasurementUnit);

        if (unit is null) return DomainErrors.MeasurementUnit.UnknownUnit;

        var product = new Product()
        {
            Name = context.Name,
            Description = context.Description,
            MeasurementUnitId = unit.Id,
            Owner = context.Owner
        };

        _productRepository.Add(product);

        return product;
    }

    public async Task<Result<Product>> GetAsync(Guid productId, Guid? requesterId = null)
    {
        var product = await _productRepository.GetAsync(productId);

        if (product is null) return DomainErrors.Product.NotFound;
        
        var attempt = await _productPolicy.IsRetrievalAllowedAsync(product, requesterId);

        if (attempt.IsDenied) return attempt.Error;

        return product;
    }

    public async Task<Result<IEnumerable<Product>>> GetAllAsync(GetAllProductsQueryParameters? queryParameters, Guid? requesterId)
    {
        var products = await _productRepository.GetAllAsync(queryParameters);

        //TODO: move filter to DB side!!!!!
        var allowedProducts = products.Where(_productPolicy.GetIsProductRetrievalAllowedPredicate(requesterId));

        return allowedProducts.ToList();
    }

    public async Task<Result<Product>> AddInventoryRecordAsync(AddProductInventoryContext context)
    {
        var product = await GetAsync(context.ProductId, context.RequesterId);

        if (product.IsFailure) return product.Error;

        var attempt = await _productPolicy.IsInventoryRecordCreationAllowedAsync(product, context);

        if (attempt.IsDenied) return attempt.Error;

        var inventoryRecord = new InventoryRecord()
        {
            ProductId = context.ProductId,
            UnitPrice = context.Price,
            Quantity = context.Quantity
        };

        product.Value.InventoryRecords.Add(inventoryRecord);

        return product;
    }

    public async Task<Result<Product>> UpdateAsync(UpdateProductContext context)
    {
        var unit = MeasurementUnit.GetByName(context.MeasurementUnit);

        if (unit is null) return DomainErrors.MeasurementUnit.UnknownUnit;

        var product = await GetAsync(context.ProductId, context.RequesterId);

        if (product.IsFailure) return product.Error;

        var attempt = await _productPolicy.IsUpdateAllowedAsync(product, context);

        if (attempt.IsDenied) return attempt.Error;

        product.Value.Name = context.Name;
        product.Value.Description = context.Description;
        product.Value.MeasurementUnitId = unit.Id;

        return product;
    }

    public async Task<Result> DeleteAsync(Guid productId, Guid requesterId)
    {
        var product = await GetAsync(productId, requesterId);

        if (product.IsFailure) return Result.Failure(product.Error);

        var attempt = await _productPolicy.IsDeleteAllowedAsync(product, requesterId);

        if (attempt.IsDenied) return Result.Failure(attempt.Error);

        _productRepository.Remove(product);

        return Result.Success();
    }
}