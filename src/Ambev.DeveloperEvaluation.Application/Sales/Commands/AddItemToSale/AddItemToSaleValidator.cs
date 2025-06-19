using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.AddItemToSale
{
    public class AddItemToSaleValidator : AbstractValidator<AddItemToSaleCommand>
    {
        public AddItemToSaleValidator()
        {
            RuleFor(command => command.SaleId)
                .NotEmpty().WithMessage("Sale ID is required.");

            RuleFor(command => command.Item)
                .NotNull().WithMessage("Item details are required.")
                .SetValidator(new AddItemToSaleCommandItemValidator());
        }
    }

    public class AddItemToSaleCommandItemValidator : AbstractValidator<AddItemToSaleCommand.AddItemToSaleCommandItem>
    {
        public AddItemToSaleCommandItemValidator()
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