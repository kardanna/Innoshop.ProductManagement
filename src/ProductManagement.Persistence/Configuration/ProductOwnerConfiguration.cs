using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Persistence.Configuration;

public class ProductOwnerConfiguration : IEntityTypeConfiguration<ProductOwner>
{
    public void Configure(EntityTypeBuilder<ProductOwner> builder)
    {
        builder.ToTable("ProductOwner", "Inventory");

        builder.HasIndex(o => o.Id);
        builder.Property(o => o.Id)
            .ValueGeneratedNever();
        
        builder.Property(o => o.IsDeactivated)
            .HasDefaultValue(false);
        
        builder.Property(o => o.IsDeleted)
            .HasDefaultValue(false);

        builder.HasMany(o => o.Products)
            .WithOne(p => p.Owner);
        
        builder.HasData(
            [
                new()
                {
                    Id = Guid.Parse("160be924-907f-4d70-d15c-08de2383d454"),
                    IsDeactivated = false,
                    IsDeleted = false
                }
            ]
        );
    }
}