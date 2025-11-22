using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using ProductManagement.Application.UseCases.Products.Get;
using ProductManagement.Application.UseCases.Products.GetAll;

namespace ProductManagement.Presentation.Controllers;

[Route("products")]
public class GetProductsController : BaseApiController
{
    private readonly ILogger<GetProductsController> _logger;

    public GetProductsController(
        ILogger<GetProductsController> logger,
        ISender sender)
        : base(sender)
    {
        _logger = logger;
    }

    [HttpGet("{id}:guid")]
    public async Task<IActionResult> GetById(Guid id)
    {
        await HttpContext.AuthenticateAsync();

        var requesterId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;

        _ = Guid.TryParse(requesterId, out var requesterGuid);

        var command = new GetProductQuery(
            productId: id,
            requesterId: requesterGuid
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok(response.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts([FromQuery] GetAllProductsQueryParameters queryParameters)
    {
        await HttpContext.AuthenticateAsync();

        var requesterId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)
            ?.Value;

        _ = Guid.TryParse(requesterId, out var requesterGuid);

        var command = new GetAllProductsQuery(
            queryParameters: queryParameters,
            requesterId: requesterGuid
        );

        var response = await _sender.Send(command);

        if (response.IsFailure) return HandleFailure(response);

        return Ok(response.Value);
    }
}