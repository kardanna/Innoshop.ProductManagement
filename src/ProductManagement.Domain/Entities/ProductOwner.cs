namespace ProductManagement.Domain.Entities;

public class ProductOwner
{
    public Guid Id { get; set; }
    public bool IsDeactivated { get; set; }
    public bool IsDeleted { get; set; }
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}