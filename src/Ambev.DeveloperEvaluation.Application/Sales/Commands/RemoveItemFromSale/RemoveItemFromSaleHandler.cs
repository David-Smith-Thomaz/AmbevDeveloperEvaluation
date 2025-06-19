using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.RemoveItemFromSale
{
    public class RemoveItemFromSaleHandler : IRequestHandler<RemoveItemFromSaleCommand, Unit>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMediator _mediator;

        public RemoveItemFromSaleHandler(ISaleRepository saleRepository, IMediator mediator)
        {
            _saleRepository = saleRepository;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(RemoveItemFromSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetByIdAsync(request.SaleId);

            if (sale == null)
            {
                throw new InvalidOperationException($"Sale with ID {request.SaleId} not found.");
            }
            if (sale.Status == SaleStatus.Cancelled)
            {
                throw new DomainValidationException("Cannot remove items from a cancelled sale.");
            }

            var itemRemoved = sale.RemoveItem(request.ItemId);

            if (itemRemoved)
            {
                await _saleRepository.UpdateAsync(sale);
                await _mediator.Publish(new SaleModifiedEvent(sale.Id), cancellationToken);
                Console.WriteLine($"[Event] Item removed from Sale ID: {sale.Id}, Item ID: {request.ItemId}");
            }
            else
            {
                Console.WriteLine($"[Info] Item ID: {request.ItemId} not found in Sale ID: {sale.Id}. No changes applied.");
            }

            return Unit.Value;
        }
    }
}