using ProductManagement.Application.UseCases.Products.GetAll;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Repositories;

public interface IProductRepository
{
    void Add(Product product);
    Task<Product?> GetAsync(Guid id);
    Task<IEnumerable<Product>> GetAllAsync(GetAllProductsQueryParameters? parameters);
    void Remove(Guid id);
    void Remove(Product product);
}