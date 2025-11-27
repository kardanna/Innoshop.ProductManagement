using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Contexts;

public class AddProductContext
{
    public ProductOwner Owner { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string MeasurementUnit { get; init; }

    public AddProductContext(
        ProductOwner owner,
        string name,
        string description,
        string measurementUnit)
    {
        Owner = owner;
        Name = name;
        Description = description;
        MeasurementUnit = measurementUnit;
    }
}