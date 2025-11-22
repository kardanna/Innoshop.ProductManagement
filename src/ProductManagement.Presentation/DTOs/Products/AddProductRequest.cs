namespace ProductManagement.Presentation.DTOs.Products;

public class AddProductRequest
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string MeasurementUnit { get; set; } = null!;
}