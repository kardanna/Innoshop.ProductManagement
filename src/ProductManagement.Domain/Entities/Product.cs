namespace ProductManagement.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int MeasurementUnitId { get; set; }
    public MeasurementUnit MeasurementUnit { get; set; } = null!;
    public virtual ICollection<InventoryRecord> InventoryRecords { get; set; } = new List<InventoryRecord>();
    public Guid OwnerId { get; set; }
    public ProductOwner Owner { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
}