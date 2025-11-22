using System.ComponentModel;
using System.Globalization;
using System.Text.Json.Serialization;

namespace ProductManagement.Application.UseCases.Products.GetAll;

[TypeConverter(typeof(ProductOrderByConverter))]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductOrderBy
{
    Name,
    MinPrice,
    MaxPrice,
    Quantity
}

public class ProductOrderByConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string stringValue)
        {
            if (!Enum.TryParse<ProductOrderBy>(stringValue, true, out var orderBy))
            {
                throw new ArgumentException($"Invalid '{nameof(ProductOrderBy)}' value '{stringValue}'. Allowed values: Name, Price, Quantity.");
            }
            return orderBy;
        }

        return base.ConvertFrom(context, culture, value);
    }
}