using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.UpdateSale
{
    public class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
    {
        public UpdateSaleValidator()
        {
            RuleFor(command => command.Id)
                .NotEmpty().WithMessage("Sale ID is required.");

            RuleFor(command => command.CustomerName)
                .NotEmpty().WithMessage("Customer name is required.");

            RuleFor(command => command.BranchName)
                .NotEmpty().WithMessage("Branch name is required.");
        }
    }
}