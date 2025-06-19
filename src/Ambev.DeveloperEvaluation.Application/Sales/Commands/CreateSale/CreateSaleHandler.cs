using MediatR;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale
{
    public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, Guid>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMediator _mediator;

        public CreateSaleHandler(ISaleRepository saleRepository, IMediator mediator)
        {
            _saleRepository = saleRepository;
            _mediator = mediator;
        }

        public async Task<Guid> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = new Sale(
                request.SaleNumber,
                request.SaleDate,
                request.CustomerId,
                request.CustomerName,
                request.BranchId,
                request.BranchName
            );

            foreach (var itemRequest in request.Items)
            {
                var saleItem = new SaleItem(
                    itemRequest.ProductId,
                    itemRequest.ProductName,
                    itemRequest.Quantity,
                    itemRequest.UnitPrice
                );
                sale.AddItem(saleItem);
            }

            // A validação de domínio vai lançar DomainValidationException diretamente.
            await sale.ValidateAsync(); // <-- Chama o método que agora lança DomainValidationException

            await _saleRepository.AddAsync(sale);

            await _mediator.Publish(new SaleCreatedEvent(sale.Id), cancellationToken);
            Console.WriteLine($"[Event] SaleCreatedEvent published for Sale ID: {sale.Id}");

            return sale.Id;
        }
    }
}