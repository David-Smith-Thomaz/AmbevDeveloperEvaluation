using Ambev.DeveloperEvaluation.Application.Sales.Commands.AddItemToSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using MediatR; 
using NSubstitute;
using Xunit;
using FluentAssertions;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales
{
    /// <summary>
    /// Contains unit tests for the <see cref="AddItemToSaleHandler"/> class.
    /// </summary>
    public class AddItemToSaleHandlerTests
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMediator _mediator;
        private readonly AddItemToSaleHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddItemToSaleHandlerTests"/> class.
        /// Sets up the test dependencies.
        /// </summary>
        public AddItemToSaleHandlerTests()
        {
            _saleRepository = Substitute.For<ISaleRepository>();
            _mediator = Substitute.For<IMediator>();
            _handler = new AddItemToSaleHandler(_saleRepository, _mediator);
        }

        /// <summary>
        /// Tests that a valid request adds an item to an existing sale.
        /// </summary>
        [Fact(DisplayName = "Given valid item data When adding item to sale Then item is added and sale is modified")]
        public async Task Handle_ValidRequest_AddsItemToSale()
        {
            // Given
            var saleId = Guid.NewGuid();
            var initialSale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId, SaleStatus.Active);
            var initialTotalAmount = initialSale.TotalAmount;
            var initialItemCount = initialSale.SaleItems.Count;

            var itemCommandData = new AddItemToSaleCommand.AddItemToSaleCommandItem
            {
                ProductId = "NEW-PROD-001",
                ProductName = "Novo Item Teste",
                Quantity = 5,
                UnitPrice = 10.00m
            };
            var command = new AddItemToSaleCommand { SaleId = saleId, Item = itemCommandData };

            _saleRepository.GetByIdAsync(saleId).Returns(initialSale);
            _saleRepository.UpdateAsync(Arg.Any<Sale>()).Returns(Task.CompletedTask);

            // When
            var newItemId = await _handler.Handle(command, CancellationToken.None);

            // Then
            newItemId.Should().NotBeEmpty();

            await _saleRepository.Received(1).GetByIdAsync(saleId);
            await _saleRepository.Received(1).UpdateAsync(Arg.Is<Sale>(s =>
                s.Id == saleId &&
                s.SaleItems.Count == initialItemCount + 1 && 
                s.SaleItems.Any(item => item.Id == newItemId && item.ProductId == itemCommandData.ProductId) &&
                s.TotalAmount > initialTotalAmount 
            ));

            await _mediator.Received(1).Publish(Arg.Is<SaleModifiedEvent>(e => e.SaleId == saleId), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Tests that adding an item to a non-existent sale throws an InvalidOperationException.
        /// </summary>
        [Fact(DisplayName = "Given non-existent sale ID When adding item Then throws InvalidOperationException")]
        public async Task Handle_NonExistentSale_ThrowsInvalidOperationException()
        {
            // Given
            var saleId = Guid.NewGuid();
            var itemCommandData = new Ambev.DeveloperEvaluation.Application.Sales.Commands.AddItemToSale.AddItemToSaleCommand.AddItemToSaleCommandItem
            { ProductId = "PROD-001", Quantity = 1, UnitPrice = 1.00m };
            var command = new AddItemToSaleCommand { SaleId = saleId, Item = itemCommandData };

            _saleRepository.GetByIdAsync(saleId).Returns((Sale?)null);

            // When
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Then
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Sale with ID {saleId} not found.");

            await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
            await _mediator.DidNotReceive().Publish(Arg.Any<SaleModifiedEvent>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Tests that adding an item to a cancelled sale throws an InvalidOperationException.
        /// </summary>
        [Fact(DisplayName = "Given cancelled sale When adding item Then throws InvalidOperationException")]
        public async Task Handle_CancelledSale_ThrowsInvalidOperationException()
        {
            // Given
            var saleId = Guid.NewGuid();
            var cancelledSale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId, SaleStatus.Cancelled);
            var itemCommandData = new Ambev.DeveloperEvaluation.Application.Sales.Commands.AddItemToSale.AddItemToSaleCommand.AddItemToSaleCommandItem
            { ProductId = "PROD-001", Quantity = 1, UnitPrice = 1.00m };
            var command = new AddItemToSaleCommand { SaleId = saleId, Item = itemCommandData };

            _saleRepository.GetByIdAsync(saleId).Returns(cancelledSale);

            // When
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Then
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot add items to a cancelled sale.");

            await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
            await _mediator.DidNotReceive().Publish(Arg.Any<SaleModifiedEvent>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Tests that adding an item with invalid quantity throws a DomainValidationException.
        /// </summary>
        [Fact(DisplayName = "Given invalid item quantity When adding item to sale Then throws DomainValidationException")]
        public async Task Handle_InvalidItemQuantity_ThrowsDomainValidationException()
        {
            // Given
            var saleId = Guid.NewGuid();
            var activeSale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId, SaleStatus.Active);
            var itemCommandData = new Ambev.DeveloperEvaluation.Application.Sales.Commands.AddItemToSale.AddItemToSaleCommand.AddItemToSaleCommandItem
            { ProductId = "PROD-001", Quantity = 21, UnitPrice = 1.00m };
            var command = new AddItemToSaleCommand { SaleId = saleId, Item = itemCommandData };

            _saleRepository.GetByIdAsync(saleId).Returns(activeSale);

            // When
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Then
            var exception = await act.Should().ThrowAsync<DomainValidationException>();

            exception.Which.Errors.Should().NotBeEmpty();
            exception.Which.Errors.Should().Contain(e => e.Detail.Contains("Cannot sell more than 20 identical items."));

            await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
            await _mediator.DidNotReceive().Publish(Arg.Any<SaleModifiedEvent>(), Arg.Any<CancellationToken>());
        }
    }
}