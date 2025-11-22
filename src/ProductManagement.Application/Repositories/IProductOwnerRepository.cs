using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Repositories;

public interface IProductOwnerRepository
{
    Task<ProductOwner?> GetAsync(Guid id);
    void Add(ProductOwner owner);
}