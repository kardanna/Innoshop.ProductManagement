using ProductManagement.Application.Messaging;
using ProductManagement.Application.UseCases.Products.Get;

namespace ProductManagement.Application.UseCases.Products.Update;

public class UpdateProductCommand : ICommand<GetProductResponse>
{
    public Guid ProductId { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string MeasurementUnit { get; init; }
    public Guid RequesterId { get; init; }

    public UpdateProductCommand(
        Guid productId,
        string name,
        string description,
        string measurementUnit,
        Guid requesterId)
    {
        ProductId = productId;
        Name = name;
        Description = description;
        MeasurementUnit = measurementUnit;
        RequesterId = requesterId;
    }
}