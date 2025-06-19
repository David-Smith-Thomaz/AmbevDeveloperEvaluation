using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSale
{
    public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, Unit>
    {
        private readonly ISaleRepository _saleRepository;
        // private readonly IMediator _mediator; // Para publicar SaleCancelledEvent

        public CancelSaleHandler(ISaleRepository saleRepository /*, IMediator mediator */)
        {
            _saleRepository = saleRepository;
            // _mediator = mediator;
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

            // Publicar Eventos (Opcional)
            // await _mediator.Publish(new SaleCancelledEvent(sale.Id), cancellationToken);
            Console.WriteLine($"[Event] SaleCancelledEvent published for Sale ID: {sale.Id} - (Simulated Publishing)");

            return Unit.Value;
        }
    }
}