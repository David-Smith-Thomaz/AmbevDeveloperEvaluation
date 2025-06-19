using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale
{
    public class CreateSaleValidator : AbstractValidator<CreateSaleCommand>
    {
        public CreateSaleValidator()
        {
            RuleFor(command => command.Items)
                .NotEmpty().WithMessage("At least one sale item is required.")
                .Must(items => items.Any()).WithMessage("At least one sale item is required.");

            RuleForEach(command => command.Items).SetValidator(new CreateSaleCommandItemValidator());
        }
    }

    public class CreateSaleCommandItemValidator : AbstractValidator<CreateSaleCommand.CreateSaleCommandItem>
    {
        public CreateSaleCommandItemValidator()
        {
            RuleFor(item => item.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            RuleFor(item => item.ProductName)
                .NotEmpty().WithMessage("Product name is required.");

            RuleFor(item => item.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(20).WithMessage("Quantity cannot exceed 20.");

            RuleFor(item => item.UnitPrice)
                .GreaterThanOrEqualTo(0.01m).WithMessage("Unit price must be a positive number.");
        }
    }
}