using ProductManagement.Application.Contexts;
using ProductManagement.Application.Models;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Interfaces;

public interface IProductPolicy
{
    Task<PolicyResult> IsCreationAllowedAsync(AddProductContext context);
    Task<PolicyResult> IsRetrievalAllowedAsync(Product product, Guid? requesterId);
    Func<Product, bool> GetIsProductRetrievalAllowedPredicate(Guid? requesterId);
    Task<PolicyResult> IsInventoryRecordCreationAllowedAsync(Product product, AddProductInventoryContext context);
    Task<PolicyResult> IsUpdateAllowedAsync(Product product, UpdateProductContext context);
    Task<PolicyResult> IsDeleteAllowedAsync(Product product, Guid requesterId);
}