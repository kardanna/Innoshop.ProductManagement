using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using ProductManagement.API.OptionsSetup;
using ProductManagement.Application;
using ProductManagement.Infrastructure;
using ProductManagement.Persistence;
using ProductManagement.Presentation;
using Serilog;

namespace ProductManagement.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddOpenApi();

        //Policies, Services, PipelineBehaviour
        builder.Services.AddProductManagementApplication();
        
        builder.Services.AddValidatorsFromAssembly(ProductManagement.Application.AssemblyReference.Assembly,
            includeInternalTypes: true);
        
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(ProductManagement.Application.AssemblyReference.Assembly));


        //Messaging
        builder.Services.AddProductManagementInfrastructure();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme);
        

        //DbContext, UnitOfWork, Repositories
        builder.Services.AddProductManagementPersistence();
        builder.Services.AddDbContext<ApplicationContext>(options =>
        {
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("SqlServer"),
                contextOptions =>
                {
                    contextOptions.EnableRetryOnFailure(
                        maxRetryCount: 6,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    );
                });
        });


        //AddControllers, GlobalExceptionHandler
        builder.Services.AddProductManagementPresentation();
        
        
        //Options
        builder.Services.ConfigureOptions<RabbitMQOptionsSetup>();
        builder.Services.ConfigureOptions<JwtOptionsSetup>();
        builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();


        //Logging 
        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig.ReadFrom.Configuration(context.Configuration);
        });


        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            db.Database.Migrate();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseSerilogRequestLogging();

        app.UseExceptionHandler(options => { });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
