using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using ProductManagement.Application.UseCases.Products.Add;
using ProductManagement.Domain.Errors;
using ProductManagement.Domain.Shared;
using ProductManagement.Presentation.DTOs.Products;
using Innoshop.Contracts.UserManagement.UserRoles;
using ProductManagement.Presentation.Attributes;

namespace ProductManagement.Presentation.Controllers.Products;

[Route("products")]
public class AddProductController : BaseApiController
{
    private readonly ILogger<AddProductController> _logger;

    public AddProductController(
        ILogger<AddProductController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

    [HttpPost]
    [HasRole(nameof(Role.Customer))]
    public async Task<IActionResult> AddProduct([FromBody] AddProductRequest request)
    {
        var userId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }
        
        var command = new AddProductCommand(
            name: request.Name,
            description: request.Description,
            measurementUnit: request.MeasurementUnit,
            ownerId: userGuid
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return CreatedAtAction(
            actionName: nameof(GetProductsController.GetById),
            controllerName: nameof(GetProductsController).Replace("Controller", ""),
            routeValues: new { id = response.Value.Id } ,
            value: response.Value
        );
    }
}