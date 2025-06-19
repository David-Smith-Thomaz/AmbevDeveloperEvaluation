using MediatR;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.Net;

using Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.AddItemToSale;
using Ambev.DeveloperEvaluation.Application.Sales.Commands.RemoveItemFromSale;
using Ambev.DeveloperEvaluation.Application.Sales.Queries.GetSaleById;
using Ambev.DeveloperEvaluation.Application.Sales.Queries.ListSales;
using Ambev.DeveloperEvaluation.Application.Sales.Queries.GetSaleItemById;

using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.AddItemToSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSaleById;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSaleItemById;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales
{
    [ApiController]
    [Route("api/sales")]
    // [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public SalesController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        /// <summary>
        /// Cria uma nova venda.
        /// </summary>
        /// <param name="request">Dados da venda a ser criada.</param>
        /// <returns>O ID da venda criada.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CreateSaleResponse), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CreateSale([FromBody] CreateSaleRequest request)
        {
            var command = _mapper.Map<CreateSaleCommand>(request);
            var saleId = await _mediator.Send(command);

            var response = _mapper.Map<CreateSaleResponse>(saleId);
            return CreatedAtAction(nameof(GetSaleById), new { id = saleId }, response);
        }

        /// <summary>
        /// Obtém os detalhes de uma venda específica pelo seu ID.
        /// </summary>
        /// <param name="id">ID da venda.</param>
        /// <returns>Os detalhes da venda.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetSaleResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetSaleById(Guid id)
        {
            var query = new GetSaleByIdQuery { Id = id };
            var saleEntity = await _mediator.Send(query);

            if (saleEntity == null) return NotFound();

            var response = _mapper.Map<GetSaleResponse>(saleEntity);
            return Ok(response);
        }

        /// <summary>
        /// Lista vendas com opções de paginação, ordenação e filtragem.
        /// </summary>
        /// <param name="pageNumber">Número da página (padrão: 1).</param>
        /// <param name="pageSize">Número de itens por página (padrão: 10).</param>
        /// <param name="orderBy">Campo para ordenação (ex: "SaleDate desc, TotalAmount asc").</param>
        /// <param name="filter">Filtros (ex: "CustomerId=123&_minTotalAmount=100").</param>
        /// <returns>Uma lista paginada de vendas.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<GetSaleResponse>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ListSales(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? orderBy = null,
            [FromQuery] string? filter = null)
        {
            var query = new ListSalesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                OrderBy = orderBy,
                Filter = filter
            };
            var saleEntities = await _mediator.Send(query);

            var response = _mapper.Map<IEnumerable<GetSaleResponse>>(saleEntities);
            return Ok(response);
        }

        /// <summary>
        /// Atualiza os detalhes de uma venda existente.
        /// </summary>
        /// <param name="id">ID da venda a ser atualizada.</param>
        /// <param name="request">Dados para atualização.</param>
        /// <returns>No Content (204) em caso de sucesso.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateSale(Guid id, [FromBody] UpdateSaleRequest request)
        {
            var command = _mapper.Map<UpdateSaleCommand>(request);
            command.Id = id;

            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Cancela uma venda específica.
        /// </summary>
        /// <param name="id">ID da venda a ser cancelada.</param>
        /// <returns>No Content (204) em caso de sucesso.</returns>
        [HttpPut("{id}/cancel")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> CancelSale(Guid id)
        {
            var command = new CancelSaleCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Adiciona um novo item a uma venda existente.
        /// </summary>
        /// <param name="saleId">ID da venda.</param>
        /// <param name="request">Dados do item a ser adicionado.</param>
        /// <returns>O ID do item recém-adicionado.</returns>
        [HttpPost("{saleId}/items")]
        [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> AddItemToSale(Guid saleId, [FromBody] AddItemToSaleRequest request)
        {
            var command = _mapper.Map<AddItemToSaleCommand>(request);
            command.SaleId = saleId;

            var itemId = await _mediator.Send(command);
            return Created($"/api/sales/{saleId}/items/{itemId}", itemId);
        }

        /// <summary>
        /// Remove um item de uma venda existente.
        /// </summary>
        /// <param name="saleId">ID da venda.</param>
        /// <param name="itemId">ID do item a ser removido.</param>
        /// <returns>No Content (204) em caso de sucesso.</returns>
        [HttpDelete("{saleId}/items/{itemId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> RemoveItemFromSale(Guid saleId, Guid itemId)
        {
            var command = new RemoveItemFromSaleCommand { SaleId = saleId, ItemId = itemId };
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Obtém os detalhes de um item específico dentro de uma venda.
        /// </summary>
        /// <param name="saleId">ID da venda.</param>
        /// <param name="itemId">ID do item.</param>
        /// <returns>Os detalhes do item.</returns>
        [HttpGet("{saleId}/items/{itemId}")]
        [ProducesResponseType(typeof(GetSaleItemResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetSaleItemById(Guid saleId, Guid itemId)
        {
            var query = new GetSaleItemByIdQuery { SaleId = saleId, ItemId = itemId };
            var itemEntity = await _mediator.Send(query);

            if (itemEntity == null) return NotFound();

            var response = _mapper.Map<GetSaleItemResponse>(itemEntity);
            return Ok(response);
        }
    }
}