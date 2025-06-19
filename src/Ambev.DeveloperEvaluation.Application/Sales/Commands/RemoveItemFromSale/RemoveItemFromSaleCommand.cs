using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.RemoveItemFromSale
{
    public class RemoveItemFromSaleCommand : IRequest<Unit>
    {
        public Guid SaleId { get; set; }
        public Guid ItemId { get; set; }
    }
}