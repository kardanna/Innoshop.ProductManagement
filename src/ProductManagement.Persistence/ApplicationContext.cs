using Microsoft.EntityFrameworkCore;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Persistence;

public class ApplicationContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductOwner> ProductOwners { get; set; }
    public DbSet<InventoryRecord> InventoryRecords { get; set; }
    public DbSet<MeasurementUnit> MeasurementUnits { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(ProductManagement.Persistence.AssemblyReference.Assembly);
    }
}