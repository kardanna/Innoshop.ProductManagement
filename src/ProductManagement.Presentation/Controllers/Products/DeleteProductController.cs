using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using ProductManagement.Application.UseCases.Products.Delete;
using ProductManagement.Domain.Errors;
using ProductManagement.Domain.Shared;

namespace ProductManagement.Presentation.Controllers.Products;

[Route("products")]
public class DeleteProductController : BaseApiController
{
    private readonly ILogger<DeleteProductController> _logger;

    public DeleteProductController(
        ILogger<DeleteProductController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var userId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;
        
        if (!Guid.TryParse(userId, out var userGuid))
        {
            return HandleFailure(Result.Failure(DomainErrors.Authentication.InvalidSubjectClaim));
        }
        
        var command = new DeleteProductCommand(
            productId: id,
            requesterId: userGuid
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok();
    }
}