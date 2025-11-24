using Microsoft.EntityFrameworkCore;
using ProductManagement.Application.Repositories;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Persistence.Repositories;

public class ProductOwnerRepository : IProductOwnerRepository
{
    private readonly ApplicationContext _appContext;

    public ProductOwnerRepository(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public void Add(ProductOwner owner)
    {
        _appContext.ProductOwners.Add(owner);
    }

    public async Task<ProductOwner?> GetAsync(Guid id)
    {
        return await _appContext.ProductOwners
            .FirstOrDefaultAsync(o => o.Id == id); 
    }
}