using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Persistence.Configuration;

public class InventoryRecordConfiguration : IEntityTypeConfiguration<InventoryRecord>
{
    public void Configure(EntityTypeBuilder<InventoryRecord> builder)
    {
        builder.ToTable("Record", "Inventory");

        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Product)
            .WithMany(p => p.InventoryRecords)
            .HasForeignKey(r => r.ProductId);
        
        builder.Property(r => r.UnitPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(r => r.Quantity)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
        
        builder.HasData(
            [
                new()
                {
                    Id = Guid.Parse("d848715a-ca18-47b2-be95-a3ca5325848c"),
                    ProductId = Guid.Parse("db976302-1ffe-4993-85f7-3ac1397cc5be"),
                    UnitPrice = 80.70M,
                    Quantity = 3
                },
                new()
                {
                    Id = Guid.Parse("9fa64946-0b65-44f8-959c-d58360918180"),
                    ProductId = Guid.Parse("db976302-1ffe-4993-85f7-3ac1397cc5be"),
                    UnitPrice = 93.99M,
                    Quantity = 20
                }
            ]
        );
    }
}