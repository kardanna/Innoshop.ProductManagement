using ProductManagement.Application.Contexts;
using ProductManagement.Application.UseCases.Products.GetAll;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.Interfaces;

public interface IProductService
{
    Task<Result<Product>> AddAsync(AddProductContext context);
    Task<Result<Product>> GetAsync(Guid productId, Guid? requesterId);
    Task<Result<IEnumerable<Product>>> GetAllAsync(GetAllProductsQueryParameters? queryParameters, Guid? requesterId);
    Task<Result<Product>> AddInventoryRecordAsync(AddProductInventoryContext context);
    Task<Result<Product>> UpdateAsync(UpdateProductContext context);
    Task<Result> DeleteAsync(Guid productId, Guid requesterId);
}