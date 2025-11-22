using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Persistence.Configuration;

public class MeasurementUnitConfiguration : IEntityTypeConfiguration<MeasurementUnit>
{
    public void Configure(EntityTypeBuilder<MeasurementUnit> builder)
    {
        builder.ToTable("MeasurementUnit", "MeasurementUnits");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.HasIndex(u => u.Name)
            .IsUnique();
        builder.Property(u => u.Name)
            .HasMaxLength(10);

        builder.HasData(MeasurementUnit.GetValues());
    }
}