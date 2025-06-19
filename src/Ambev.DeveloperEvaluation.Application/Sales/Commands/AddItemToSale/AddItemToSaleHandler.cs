using MediatR;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.AddItemToSale
{
    public class AddItemToSaleHandler : IRequestHandler<AddItemToSaleCommand, Guid>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMediator _mediator;

        public AddItemToSaleHandler(ISaleRepository saleRepository, IMediator mediator)
        {
            _saleRepository = saleRepository;
            _mediator = mediator;
        }

        public async Task<Guid> Handle(AddItemToSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetByIdAsync(request.SaleId);

            if (sale == null)
            {
                throw new InvalidOperationException($"Sale with ID {request.SaleId} not found.");
            }
            if (sale.Status == Domain.Enums.SaleStatus.Cancelled)
            {
                throw new DomainValidationException("Cannot add items to a cancelled sale.");
            }

            var newItem = new SaleItem(
                request.Item.ProductId,
                request.Item.ProductName,
                request.Item.Quantity,
                request.Item.UnitPrice
            );

            var itemValidationErrors = await newItem.ValidateAsync();
            if (itemValidationErrors.Any())
            {
                throw new InvalidOperationException($"Erro de validação do item: {string.Join("; ", itemValidationErrors.Select(e => e.Error))}");
            }

            sale.AddItem(newItem);

            var saleValidationErrors = await sale.ValidateAsync();
            if (saleValidationErrors.Any())
            {
                throw new InvalidOperationException($"Erro de validação da venda ao adicionar item: {string.Join("; ", saleValidationErrors.Select(e => e.Error))}");
            }

            await _saleRepository.UpdateAsync(sale);
            await _mediator.Publish(new SaleModifiedEvent(sale.Id), cancellationToken);

            Console.WriteLine($"[Event] Item added to Sale ID: {sale.Id}, Item ID: {newItem.Id} - (Simulated Publishing)"); // Pode remover esta linha.

            return newItem.Id;
        }
    }
}