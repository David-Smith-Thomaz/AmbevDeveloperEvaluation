using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.UpdateSale
{
    public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, Unit>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMediator _mediator;

        public UpdateSaleHandler(ISaleRepository saleRepository, IMediator mediator)
        {
            _saleRepository = saleRepository;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = await _saleRepository.GetByIdAsync(request.Id);

            if (sale == null)
            {
                throw new InvalidOperationException($"Sale with ID {request.Id} not found.");
            }
            if (sale.Status == Domain.Enums.SaleStatus.Cancelled)
            {
                throw new DomainValidationException("Cannot update details of a cancelled sale.");
            }

            sale.UpdateSaleDetails(request.CustomerName, request.BranchName);

            await sale.ValidateAsync();
            await _saleRepository.UpdateAsync(sale);
            await _mediator.Publish(new SaleModifiedEvent(sale.Id), cancellationToken);

            return Unit.Value;
        }
    }
}