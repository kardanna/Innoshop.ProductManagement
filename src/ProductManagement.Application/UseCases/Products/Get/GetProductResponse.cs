using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.UseCases.Products.Get;

public class GetProductResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public string MeasurementUnit { get; init; }
    public decimal? LowestPrice { get; init; }
    public decimal? TotalQuantity { get; init; }
    public IEnumerable<InventoryItem> Inventory { get; init; }
    public Guid OwnerId { get; init; }
    
    public GetProductResponse(Product product)
    {
        var inventory = product.InventoryRecords.Where(r => r.Quantity != 0);
        var isInventoryNotEmpty = inventory.Any();

        Id = product.Id;
        Name = product.Name;
        Description = product.Description;
        MeasurementUnit = product.MeasurementUnit.Name;
        LowestPrice = isInventoryNotEmpty ? inventory.Min(r => r.UnitPrice) : null;
        TotalQuantity = isInventoryNotEmpty ? inventory.Sum(r => r.Quantity) : null;
        Inventory = inventory.Select(r => new InventoryItem(r.Quantity, r.UnitPrice));
        OwnerId = product.OwnerId;
    }

    public record InventoryItem(decimal Quantity, decimal Price);
}