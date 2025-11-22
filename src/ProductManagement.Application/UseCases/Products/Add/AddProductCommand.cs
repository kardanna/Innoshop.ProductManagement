using ProductManagement.Application.Messaging;
using ProductManagement.Application.UseCases.Products.Get;

namespace ProductManagement.Application.UseCases.Products.Add;

public class AddProductCommand : ICommand<GetProductResponse>
{
    public string Name { get; init; }
    public string Description { get; init; }
    public string MeasurementUnit { get; init; }
    public Guid OwnerId { get; init; }

    public AddProductCommand(
        string name,
        string description,
        string measurementUnit,
        Guid ownerId)
    {
        Name = name;
        Description = description;
        MeasurementUnit = measurementUnit;
        OwnerId = ownerId;
    }
}