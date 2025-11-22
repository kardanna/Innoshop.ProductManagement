namespace ProductManagement.Presentation.DTOs.Products;

public class UpdateProductRequest
{
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string MeasurementUnit { get; init; } = null!;
}