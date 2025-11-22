using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.UseCases.Products.Get;

public class GetProductResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string MeasurementUnit { get; init; }
    public decimal LowestPrice { get; init; }
    public decimal TotalQuantity { get; init; }
    public IEnumerable<InventoryItem> Inventory { get; init; }
    public Guid OwnerId { get; init; }
    
    public GetProductResponse(Product product)
    {
        Id = product.Id;
        Name = product.Name;
        Description = product.Description;
        MeasurementUnit = product.MeasurementUnit.Name;
        LowestPrice = product.InventoryRecords.Min(r => r.UnitPrice);
        TotalQuantity = product.InventoryRecords.Sum(r => r.Quantity);
        Inventory = product.InventoryRecords.Select(r => new InventoryItem(r.Quantity, r.UnitPrice));
        OwnerId = product.OwnerId;
    }

    public record InventoryItem(decimal Quantity, decimal Price);
}