using Microsoft.EntityFrameworkCore;
using ProductManagement.Application.Interfaces;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationContext _appContext;

    public UnitOfWork(ApplicationContext appContext)
    {
        _appContext = appContext;
    }

    public async Task SaveChangesAsync()
    {
        /*var measurementUnits = _appContext.ChangeTracker
            .Entries<MeasurementUnit>()
            .Where(e => e.State == EntityState.Added);
        
        foreach (var measurementUnit in measurementUnits)
        {
            measurementUnit.State = EntityState.Unchanged;
        }*/

        var createdProducts = _appContext.ChangeTracker
            .Entries<Product>()
            .Where(e => e.State == EntityState.Added);
        
        foreach (var product in createdProducts)
        {
            product.Entity.CreatedAt = DateTime.UtcNow;
        }

        var modifiedProducts = _appContext.ChangeTracker
            .Entries<Product>()
            .Where(e => e.State == EntityState.Modified);
        
        foreach (var product in modifiedProducts)
        {
            product.Entity.LastModifiedAt = DateTime.UtcNow;
        }

        await _appContext.SaveChangesAsync();
    }
}