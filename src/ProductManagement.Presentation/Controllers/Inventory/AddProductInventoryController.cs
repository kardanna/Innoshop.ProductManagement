using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using ProductManagement.Application.UseCases.ProductInventory.Add;
using ProductManagement.Domain.Errors;
using ProductManagement.Domain.Shared;
using ProductManagement.Presentation.Controllers.Products;
using ProductManagement.Presentation.DTOs.ProductInventory;
using ProductManagement.Presentation.Attributes;
using Innoshop.Contracts.UserManagement.UserRoles;

namespace ProductManagement.Presentation.Controllers.Inventory;

[Route("inventory")]
public class AddProductInventoryController : BaseApiController
{
    private readonly ILogger<AddProductInventoryController> _logger;

    public AddProductInventoryController(
        ILogger<AddProductInventoryController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

    [HttpPost]
    [HasRole(nameof(Role.Customer))]
    public async Task<IActionResult> AddInventory([FromBody] AddProductInventoryRequest request)
    {
        var userId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }
        
        var command = new AddProductInventoryCommand(
            productId: request.ProductId,
            price: request.Price,
            quantity: request.Quantity,
            requseterId: userGuid
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