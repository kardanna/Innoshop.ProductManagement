using ProductManagement.Application.Contexts;
using ProductManagement.Application.Models;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Interfaces;

public interface IProductPolicy
{
    Task<PolicyResult> IsAddingAllowedAsync(AddProductContext context);
    Task<PolicyResult> IsRetrievalAllowedAsync(Product product, Guid? requesterId);
    Func<Product, bool> GetIsProductRetrievalAllowedPredicate(Guid? requesterId);
}