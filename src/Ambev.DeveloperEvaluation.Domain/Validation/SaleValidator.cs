using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

public class SaleValidator : AbstractValidator<Sale>
{
    public SaleValidator()
    {
        RuleFor(sale => sale.SaleNumber)
            .NotEmpty().WithMessage("Sale number cannot be empty.")
            .MaximumLength(50).WithMessage("Sale number cannot exceed 50 characters.");

        RuleFor(sale => sale.CustomerId)
            .NotEmpty().WithMessage("Customer ID cannot be empty.");

        RuleFor(sale => sale.CustomerName)
            .NotEmpty().WithMessage("Customer name cannot be empty.");

        RuleFor(sale => sale.BranchId)
            .NotEmpty().WithMessage("Branch ID cannot be empty.");

        RuleFor(sale => sale.BranchName)
            .NotEmpty().WithMessage("Branch name cannot be empty.");

        RuleFor(sale => sale.SaleDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Sale date cannot be in the future.");

        RuleFor(sale => sale.SaleItems)
            .Must(items => items != null && items.Any()).WithMessage("A sale must have at least one item.");

        RuleForEach(sale => sale.SaleItems).SetValidator(new SaleItemValidator());
    }
}