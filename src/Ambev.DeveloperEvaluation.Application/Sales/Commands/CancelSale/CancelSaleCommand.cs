using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSale
{
    public class CancelSaleCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
    }
}