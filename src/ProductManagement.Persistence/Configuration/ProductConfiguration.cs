using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Persistence.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product", "Inventory");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(50);
        
        builder.Property(p => p.Description)
            .HasMaxLength(256);

        builder.HasMany(p => p.InventoryRecords)
            .WithOne(r => r.Product)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(p => p.MeasurementUnit)
            .WithMany()
            .HasForeignKey(p => p.MeasurementUnitId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(p => p.Owner)
            .WithMany(o => o.Products)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasData(
            [
                new()
                {
                    Id = Guid.Parse("db976302-1ffe-4993-85f7-3ac1397cc5be"),
                    Name = "Перчатки",
                    Description = "Пара кожанных перчаток с заячьим мехом",
                    MeasurementUnitId = 1,
                    OwnerId = Guid.Parse("160be924-907f-4d70-d15c-08de2383d454")
                }
            ]
        );
    }
}