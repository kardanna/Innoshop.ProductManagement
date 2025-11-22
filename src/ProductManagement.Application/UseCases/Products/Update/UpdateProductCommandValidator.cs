using System.Data;
using FluentValidation;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.UseCases.Products.Update;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(50);
        
        RuleFor(c => c.Description)
            .NotEmpty()
            .MaximumLength(256);
        
        RuleFor(c => c.MeasurementUnit)
            .NotEmpty()
            .Must(u => MeasurementUnit.GetValues().Select(mu => mu.Name).Contains(u)).WithMessage("Unknown measurment unit");
    }
}