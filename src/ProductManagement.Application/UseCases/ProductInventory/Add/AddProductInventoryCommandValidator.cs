using FluentValidation;
using ProductManagement.Application.UseCases.ProductInventory.Add;

namespace ProductManagement.Application.UseCases.Products.Add;

public class AddProductInventoryCommandValidator : AbstractValidator<AddProductInventoryCommand>
{
    public AddProductInventoryCommandValidator()
    {
        RuleFor(c => c.Price)
            .GreaterThan(0);
        
        RuleFor(c => c.Quantity)
            .GreaterThan(0);
    }
}