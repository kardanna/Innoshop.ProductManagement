using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.Interfaces;

public interface IProductOwnerService
{
    Task<Result<ProductOwner>> GetOrCreateProductOwnerAsync(Guid ownerId);
    Task<Result> DeactivateProductOwnerAsync(Guid ownerId);
    Task<Result> ReactivateProductOwnerAsync(Guid ownerId);
    Task<Result> DeleteProductOwnerAsync(Guid ownerId);
}