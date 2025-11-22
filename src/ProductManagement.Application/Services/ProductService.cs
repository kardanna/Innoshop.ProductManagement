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
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        IProductRepository productRepository,
        IProductPolicy productPolicy,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _productPolicy = productPolicy;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Product>> AddAsync(AddProductContext context)
    {
        var attempt = await _productPolicy.IsAddingAllowedAsync(context);

        if (attempt.IsDenied) return attempt.Error;

        var product = new Product()
        {
            Name = context.Name,
            Description = context.Description,
            MeasurementUnit = context.MeasurementUnit,
            Owner = context.Owner
        };

        _productRepository.Add(product);

        await _unitOfWork.SaveChangesAsync();

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

        var allowedProducts = products.Where(_productPolicy.GetIsProductRetrievalAllowedPredicate(requesterId));

        return allowedProducts.ToList();
    }
}