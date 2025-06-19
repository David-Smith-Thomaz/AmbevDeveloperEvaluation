using FluentValidation;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Validation
{
    public class SaleItemValidator : AbstractValidator<SaleItem>
    {
        public SaleItemValidator()
        {
            RuleFor(item => item.ProductId)
                .NotEmpty().WithMessage("Product ID cannot be empty.");

            RuleFor(item => item.ProductName)
                .NotEmpty().WithMessage("Product name cannot be empty.");

            RuleFor(item => item.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(20).WithMessage("Quantity cannot exceed 20.");

            RuleFor(item => item.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be a non-negative number.");

            RuleFor(item => item)
                .Must(item => (item.Quantity >= 10 && item.Discount > 0) ||
                              (item.Quantity >= 4 && item.Quantity < 10 && item.Discount > 0) ||
                              (item.Quantity < 4 && item.Discount == 0))
                .WithMessage("Discount rules violated for the item's quantity.")
                .When(item => !item.IsCancelled);
        }
    }
}