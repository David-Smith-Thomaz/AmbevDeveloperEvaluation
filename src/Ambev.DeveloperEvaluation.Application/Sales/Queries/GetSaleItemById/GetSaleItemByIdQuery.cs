using MediatR;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries.GetSaleItemById
{
    public class GetSaleItemByIdQuery : IRequest<SaleItem?>
    {
        public Guid SaleId { get; set; }
        public Guid ItemId { get; set; }
    }
}