using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.RemoveItemFromSale
{
    public class RemoveItemFromSaleHandler : IRequestHandler<RemoveItemFromSaleCommand, Unit>
    {
        private readonly ISaleRepository _saleRepository;
        // private readonly IMediator _mediator; // Para publicar SaleModifiedEvent e ItemRemovedEvent

        public RemoveItemFromSaleHandler(ISaleRepository saleRepository /*, IMediator mediator */)
        {
            _saleRepository = saleRepository;
            // _mediator = mediator;
        }

        public async Task<Unit> Handle(RemoveItemFromSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetByIdAsync(request.SaleId);

            if (sale == null)
            {
                throw new InvalidOperationException($"Sale with ID {request.SaleId} not found.");
            }
            if (sale.Status == Domain.Enums.SaleStatus.Cancelled)
            {
                throw new InvalidOperationException("Cannot remove items from a cancelled sale.");
            }

            // A lógica de remoção está na entidade de domínio
            sale.RemoveItem(request.ItemId);

            // Validação da Sale após remover o item
            var saleValidationErrors = await sale.ValidateAsync();
            if (saleValidationErrors.Any())
            {
                throw new InvalidOperationException($"Erro de validação da venda ao remover item: {string.Join("; ", saleValidationErrors.Select(e => e.Error))}");
            }

            await _saleRepository.UpdateAsync(sale);

            // Publicar Eventos (Opcional)
            // await _mediator.Publish(new SaleModifiedEvent(sale.Id), cancellationToken);
            // await _mediator.Publish(new ItemRemovedFromSaleEvent(sale.Id, request.ItemId), cancellationToken); // Se criar este evento
            Console.WriteLine($"[Event] Item removed from Sale ID: {sale.Id}, Item ID: {request.ItemId} - (Simulated Publishing)");

            return Unit.Value;
        }
    }
}