using MediatR;
using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale
{
    public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, Guid>
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMapper _mapper;
        // Se for publicar eventos de domínio (SaleCreatedEvent, etc.) via MediatR, descomente:
        // private readonly IMediator _mediator; 

        public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper /*, IMediator mediator */)
        {
            _saleRepository = saleRepository;
            _mapper = mapper;
            // _mediator = mediator;
        }

        public async Task<Guid> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
        {
            // 1. Criar a entidade de domínio Sale e seus SaleItems
            // É importante criar as entidades diretamente ou com um construtor que garanta a aplicação de regras.
            // O AutoMapper pode ser usado para mapeamento de propriedades simples, mas a lógica de AddItem
            // deve ser explícita para ativar as regras do domínio (cálculo de desconto, validações de quantidade).
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
                // Adiciona o item à venda. O método AddItem na entidade Sale
                // já chama SaleItem.SetSaleId() e recalcula o TotalAmount da Sale.
                // O construtor de SaleItem já lida com validações de quantidade e cálculo de desconto.
                sale.AddItem(saleItem);
            }

            // 2. Validar a entidade de Domínio usando o método ValidateAsync da BaseEntity
            var domainValidationErrors = await sale.ValidateAsync();
            if (domainValidationErrors.Any())
            {
                // Lançar uma exceção que será capturada pelo middleware de tratamento de erros da API.
                // Isso mapearia para um 400 Bad Request com os detalhes do erro.
                // Exemplo: throw new DomainValidationException("One or more domain validation errors occurred.", domainValidationErrors.ToList());
                throw new InvalidOperationException($"Erro de validação de domínio: {string.Join("; ", domainValidationErrors.Select(e => e.Error))}");
            }

            // 3. Persistir a Venda no repositório
            await _saleRepository.AddAsync(sale);

            // 4. Publicar Eventos de Domínio (Opcional, mas esperado pela avaliação)
            // Se você configurou o MediatR para publicar eventos de domínio:
            // await _mediator.Publish(new SaleCreatedEvent(sale.Id), cancellationToken);
            // Para simular, conforme a instrução:
            Console.WriteLine($"[Event] SaleCreatedEvent published for Sale ID: {sale.Id} - (Simulated Publishing)");

            // Retorna o ID da venda recém-criada
            return sale.Id;
        }
    }
}