using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries.ListSales
{
    public class ListSalesHandler : IRequestHandler<ListSalesQuery, IEnumerable<Sale>>
    {
        private readonly ISaleRepository _saleRepository;

        public ListSalesHandler(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository;
        }

        public async Task<IEnumerable<Sale>> Handle(ListSalesQuery request, CancellationToken cancellationToken)
        {
            var sales = await _saleRepository.ListAsync(
                request.PageNumber,
                request.PageSize,
                request.OrderBy,
                request.Filter
            );

            return sales;
        }
    }
}