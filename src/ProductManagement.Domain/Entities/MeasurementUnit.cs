using ProductManagement.Domain.Shared;

namespace ProductManagement.Domain.Entities;

public class MeasurementUnit : Enumeration<MeasurementUnit>
{
    public static readonly MeasurementUnit Unit = new(1, "шт.");
    public static readonly MeasurementUnit Meter = new(2, "м");
    public static readonly MeasurementUnit Kilogram = new(3, "кг");

    private MeasurementUnit(int id, string name)
        : base(id, name)
    {
    }

    public static implicit operator string(MeasurementUnit unit) => unit.Name;
}