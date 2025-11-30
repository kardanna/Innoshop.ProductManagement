using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProductManagement.Application.Behaviours;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Policies;
using ProductManagement.Application.Services;

namespace ProductManagement.Application;

public static class ApplicationDependencyInjection
{
    public static void AddProductManagementApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductPolicy, ProductPolicy>();
        services.AddScoped<IProductOwnerService, ProductOwnerService>();
        services.AddScoped<IProductService, ProductService>();

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PipelineValidationBehaviour<,>));
    }
}