using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSale
{
    public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, Unit>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMediator _mediator;

        public CancelSaleHandler(ISaleRepository saleRepository, IMediator mediator)
        {
            _saleRepository = saleRepository;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetByIdAsync(request.Id);

            if (sale == null)
            {
                throw new InvalidOperationException($"Sale with ID {request.Id} not found.");
            }

            sale.CancelSale();

            var domainValidationErrors = await sale.ValidateAsync();
            if (domainValidationErrors.Any())
            {
                throw new InvalidOperationException($"Erro de validação de domínio ao cancelar a venda: {string.Join("; ", domainValidationErrors.Select(e => e.Error))}");
            }

            await _saleRepository.UpdateAsync(sale);
            await _mediator.Publish(new SaleCancelledEvent(sale.Id), cancellationToken);

            Console.WriteLine($"[Event] SaleCancelledEvent published for Sale ID: {sale.Id}");

            if (sale.SaleItems != null)
            {
                foreach (var item in sale.SaleItems.Where(i => i.IsCancelled))
                {
                    await _mediator.Publish(new ItemCancelledEvent(sale.Id, item.Id), cancellationToken);
                    Console.WriteLine($"[Event] ItemCancelledEvent published for Item ID: {item.Id} in Sale ID: {sale.Id}");
                }
            }

            return Unit.Value;
        }
    }
}