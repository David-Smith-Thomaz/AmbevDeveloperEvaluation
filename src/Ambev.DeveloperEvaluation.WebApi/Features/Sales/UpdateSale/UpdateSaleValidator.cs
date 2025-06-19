using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale
{
    public class UpdateSaleValidator : AbstractValidator<UpdateSaleRequest>
    {
        public UpdateSaleValidator()
        {
            RuleFor(request => request.CustomerName)
                .NotEmpty().WithMessage("Customer name is required.");

            RuleFor(request => request.BranchName)
                .NotEmpty().WithMessage("Branch name is required.");
        }
    }
}