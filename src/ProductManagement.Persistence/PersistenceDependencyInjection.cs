using Microsoft.Extensions.DependencyInjection;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Repositories;
using ProductManagement.Persistence.Repositories;

namespace ProductManagement.Persistence;

public static class PersistenceDependencyInjection
{
    public static void AddProductManagementPersistence(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductOwnerRepository, ProductOwnerRepository>();
    }
}