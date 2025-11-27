namespace ProductManagement.Application.Contexts;

public class UpdateProductContext
{
    public Guid ProductId { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string MeasurementUnit { get; init; }
    public Guid RequesterId { get; init; }

    public UpdateProductContext(
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