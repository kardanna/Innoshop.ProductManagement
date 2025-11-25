using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Repositories;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Errors;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Application.Services;

public class ProductOwnerService : IProductOwnerService
{
    private readonly IProductOwnerRepository _productOwnerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductOwnerService(
        IProductOwnerRepository productOwnerRepository,
        IUnitOfWork unitOfWork)
    {
        _productOwnerRepository = productOwnerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductOwner>> GetOrCreateProductOwnerAsync(Guid ownerId)
    {
        var owner = await _productOwnerRepository.GetAsync(ownerId);

        if (owner is not null) return owner;

        return new ProductOwner()
        {
            Id = ownerId,
            IsDeactivated = false,
            IsDeleted = false
        };

        //save changes???
    }

    public async Task<Result> DeactivateProductOwnerAsync(Guid ownerId)
    {
        var owner = await GetOrCreateProductOwnerAsync(ownerId);

        if (owner.IsFailure) return owner;

        if (owner.Value.IsDeleted) return Result.Failure(DomainErrors.ProductOwner.Deleted);

        if (!owner.Value.IsDeactivated)
        {
            owner.Value.IsDeactivated = true;
            await _unitOfWork.SaveChangesAsync();
        }

        return Result.Success();
    }

    public async Task<Result> ReactivateProductOwnerAsync(Guid ownerId)
    {
        var owner = await GetOrCreateProductOwnerAsync(ownerId);

        if (owner.IsFailure) return owner;

        if (owner.Value.IsDeleted) return Result.Failure(DomainErrors.ProductOwner.Deleted);

        if (owner.Value.IsDeactivated)
        {
            owner.Value.IsDeactivated = false;
            await _unitOfWork.SaveChangesAsync();
        }

        return Result.Success();
    }

    public async Task<Result> DeleteProductOwnerAsync(Guid ownerId)
    {
        var owner = await GetOrCreateProductOwnerAsync(ownerId);

        if (owner.IsFailure) return owner;

        if (!owner.Value.IsDeleted)
        {
            owner.Value.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync();
            //delete all products, etc.????
        }

        return Result.Success();
    }
}