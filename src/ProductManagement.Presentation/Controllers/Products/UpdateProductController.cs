using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using ProductManagement.Application.UseCases.Products.Update;
using ProductManagement.Domain.Errors;
using ProductManagement.Domain.Shared;
using ProductManagement.Presentation.DTOs.Products;

namespace ProductManagement.Presentation.Controllers.Products;

[Route("products")]
public class UpdateProductController : BaseApiController
{
    private readonly ILogger<UpdateProductController> _logger;

    public UpdateProductController(
        ILogger<UpdateProductController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequest request, Guid id)
    {
        var userId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }
        
        var command = new UpdateProductCommand(
            productId: id,
            requesterId: userGuid,
            name: request.Name,
            description: request.Description,
            measurementUnit: request.MeasurementUnit
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok(response.Value);
    }
}