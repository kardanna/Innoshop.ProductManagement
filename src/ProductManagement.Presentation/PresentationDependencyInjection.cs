using Microsoft.Extensions.DependencyInjection;
using UserManagement.Presentation.ExceptionHandlers;

namespace ProductManagement.Presentation;

public static class PresentationDependencyInjection
{
    public static void AddProductManagementPresentation(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddControllers().AddApplicationPart(AssemblyReference.Assembly);
    }
}