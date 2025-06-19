using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.Queries.GetSaleItemById
{
    public class GetSaleItemByIdHandler : IRequestHandler<GetSaleItemByIdQuery, SaleItem?>
    {
        private readonly ISaleRepository _saleRepository;

        public GetSaleItemByIdHandler(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository;
        }

        public async Task<SaleItem?> Handle(GetSaleItemByIdQuery request, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetByIdAsync(request.SaleId);

            if (sale == null)
            {
                return null;
            }

            var item = sale.SaleItems.FirstOrDefault(si => si.Id == request.ItemId);

            if (item == null)
            {
                return null;
            }

            return item;
        }
    }
}