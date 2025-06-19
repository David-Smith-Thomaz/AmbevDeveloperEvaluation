using FluentValidation;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.AddItemToSale;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale
{
    public class CreateSaleValidator : AbstractValidator<CreateSaleRequest>
    {
        public CreateSaleValidator()
        {
            RuleFor(request => request.SaleNumber)
                .NotEmpty().WithMessage("Sale number is required.")
                .MaximumLength(50).WithMessage("Sale number cannot exceed 50 characters.");

            RuleFor(request => request.SaleDate)
                .NotEmpty().WithMessage("Sale date is required.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Sale date cannot be in the future.");

            RuleFor(request => request.CustomerId)
                .NotEmpty().WithMessage("Customer ID is required.");

            RuleFor(request => request.CustomerName)
                .NotEmpty().WithMessage("Customer name is required.");

            RuleFor(request => request.BranchId)
                .NotEmpty().WithMessage("Branch ID is required.");

            RuleFor(request => request.BranchName)
                .NotEmpty().WithMessage("Branch name is required.");

            RuleFor(request => request.Items)
                .NotEmpty().WithMessage("At least one sale item is required.")
                .Must(items => items.Any()).WithMessage("At least one sale item is required.");

            RuleForEach(request => request.Items).SetValidator(new AddItemToSaleRequestValidator());
        }
    }

    public class AddItemToSaleRequestValidator : AbstractValidator<AddItemToSaleRequest>
    {
        public AddItemToSaleRequestValidator()
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