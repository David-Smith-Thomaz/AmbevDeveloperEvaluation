using MediatR;
using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.UpdateSale
{
    public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, Unit>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;
        // private readonly IMediator _mediator; // Para publicar SaleModifiedEvent

        public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper /*, IMediator mediator */)
        {
            _saleRepository = saleRepository;
            _mapper = mapper;
            // _mediator = mediator;
        }

        public async Task<Unit> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
        {
            // 1. Buscar a venda existente
            var sale = await _saleRepository.GetByIdAsync(request.Id);

            if (sale == null)
            {
                // Lançar exceção para 404 Not Found
                throw new InvalidOperationException($"Sale with ID {request.Id} not found."); // Ou uma ResourceNotFoundException customizada
            }

            // 2. Aplicar as atualizações na entidade de domínio
            sale.UpdateSaleDetails(request.CustomerName, request.BranchName);

            // 3. Validar a entidade de Domínio após a atualização
            var domainValidationErrors = await sale.ValidateAsync();
            if (domainValidationErrors.Any())
            {
                throw new InvalidOperationException($"Erro de validação de domínio ao atualizar a venda: {string.Join("; ", domainValidationErrors.Select(e => e.Error))}");
            }

            // 4. Persistir as alterações
            await _saleRepository.UpdateAsync(sale);

            // 5. Publicar Eventos (Opcional)
            // await _mediator.Publish(new SaleModifiedEvent(sale.Id), cancellationToken);
            Console.WriteLine($"[Event] SaleModifiedEvent published for Sale ID: {sale.Id} - (Simulated Publishing)");

            return Unit.Value; // Retorna Unit.Value para indicar sucesso sem dados específicos
        }
    }
}