using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using ProductManagement.API.OptionsSetup;
using ProductManagement.Application.Behaviours;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Policies;
using ProductManagement.Application.Repositories;
using ProductManagement.Application.Services;
using ProductManagement.Infrastructure.Messaging;
using ProductManagement.Infrastructure.Messaging.Abstractions;
using ProductManagement.Persistence;
using ProductManagement.Persistence.Repositories;
using Serilog;
using UserManagement.Presentation.ExceptionHandlers;

namespace ProductManagement.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers().AddApplicationPart(ProductManagement.Presentation.AssemblyReference.Assembly);;

        builder.Services.AddOpenApi();

        builder.Services.AddDbContext<ApplicationContext>(options =>
        {
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("SqlServer"),
                contextOptions =>
                {
                    contextOptions.EnableRetryOnFailure(
                        maxRetryCount: 10,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                    );
                });
        });
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        //Scoped
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IProductOwnerRepository, ProductOwnerRepository>();
        builder.Services.AddScoped<IProductPolicy, ProductPolicy>();
        builder.Services.AddScoped<IProductOwnerService, ProductOwnerService>();
        builder.Services.AddScoped<IProductService, ProductService>();

        //Singletons

        //Scoped services

        //Configure options
        builder.Services.ConfigureOptions<JwtOptionsSetup>();
        builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();
        builder.Services.ConfigureOptions<RabbitMQOptionsSetup>();

        //Hosted services

        //RabbitMQ
        builder.Services.AddSingleton<IExchangeChannel, RabbitMQChannel>();
        builder.Services.AddHostedService<ExchangeChannelInitializer>();
        builder.Services.AddHostedService<InnoshopNotificationListener>();

        //Authentication configuration
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme);

        //Validation behaviour
        builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(PipelineValidationBehaviour<,>));
        builder.Services.AddValidatorsFromAssembly(ProductManagement.Application.AssemblyReference.Assembly,
            includeInternalTypes: true);
        

        //Global exception handler
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        //Logging 
        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig.ReadFrom.Configuration(context.Configuration);
        });

        //MediatR
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(ProductManagement.Application.AssemblyReference.Assembly));


        var app = builder.Build();


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
