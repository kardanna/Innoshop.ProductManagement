using ProductManagement.Application.Contexts;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Models;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;

namespace ProductManagement.Application.Policies;

public class ProductPolicy : IProductPolicy
{
    public async Task<PolicyResult> IsAddingAllowedAsync(AddProductContext context)
    {
        if (context.Owner.IsDeactivated) return DomainErrors.ProductOwner.Deactivated;

        if (context.Owner.IsDeleted) return DomainErrors.ProductOwner.Deleted;

        return PolicyResult.Success;
    }

    public async Task<PolicyResult> IsRetrievalAllowedAsync(Product product, Guid? requesterId = null)
    {
        var theRequesterIsTheProductOwner = product.OwnerId == requesterId;

        if (product.Owner.IsDeactivated && !theRequesterIsTheProductOwner) return DomainErrors.Product.NotFound;

        if (product.Owner.IsDeleted) return DomainErrors.Product.NotFound; //Remove. Just delete the products? 

        return PolicyResult.Success;
    }

    public Func<Product, bool> GetIsProductRetrievalAllowedPredicate(Guid? requesterId = null)
    {
        var predicate = (Product p) =>
        {
            var theRequesterIsTheProductOwner = p.OwnerId == requesterId;
            return !p.Owner.IsDeactivated || p.Owner.IsDeactivated && theRequesterIsTheProductOwner;// || p.Owner.IsDeleted;
        };

        return predicate;
    }
}