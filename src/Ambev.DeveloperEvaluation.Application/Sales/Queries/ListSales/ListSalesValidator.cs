using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries.ListSales
{
    public class ListSalesValidator : AbstractValidator<ListSalesQuery>
    {
        public ListSalesValidator()
        {
            RuleFor(query => query.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

            RuleFor(query => query.PageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
        }
    }
}