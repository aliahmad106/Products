using FluentValidation;
using Products.Application.DTOs;

namespace Products.Application.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must be greater than or equal to 0.");

        RuleFor(x => x.Colour)
            .NotEmpty()
            .WithMessage("Colour is required.");
    }
}
