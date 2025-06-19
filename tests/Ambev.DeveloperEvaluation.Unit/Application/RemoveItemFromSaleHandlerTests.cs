using Ambev.DeveloperEvaluation.Application.Sales.Commands.RemoveItemFromSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using MediatR;
using NSubstitute;
using Xunit;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales
{
    /// <summary>
    /// Contains unit tests for the <see cref="RemoveItemFromSaleHandler"/> class.
    /// </summary>
    public class RemoveItemFromSaleHandlerTests
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMediator _mediator;
        private readonly RemoveItemFromSaleHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveItemFromSaleHandlerTests"/> class.
        /// Sets up the test dependencies.
        /// </summary>
        public RemoveItemFromSaleHandlerTests()
        {
            _saleRepository = Substitute.For<ISaleRepository>();
            _mediator = Substitute.For<IMediator>();
            _handler = new RemoveItemFromSaleHandler(_saleRepository, _mediator);
        }

        /// <summary>
        /// Tests that a valid request removes an item from an existing sale.
        /// </summary>
        [Fact(DisplayName = "Given valid item ID When removing item from sale Then item is removed and sale is modified")]
        public async Task Handle_ValidRequest_RemovesItemFromSale()
        {
            // Given
            var saleId = Guid.NewGuid();
            var itemToRemoveId = Guid.NewGuid();
            var saleWithItems = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId, SaleStatus.Active);

            var itemToRemove = new SaleItem("REMOVE-PROD-001", "Item to Remove", 1, 10.00m);
            itemToRemove.GetType().GetProperty(nameof(SaleItem.Id))?.SetValue(itemToRemove, itemToRemoveId);
            saleWithItems.AddItem(itemToRemove);

            var initialItemCount = saleWithItems.SaleItems.Count;
            var initialTotalAmount = saleWithItems.TotalAmount;

            var command = new RemoveItemFromSaleCommand { SaleId = saleId, ItemId = itemToRemoveId };

            _saleRepository.GetByIdAsync(saleId).Returns(saleWithItems);
            _saleRepository.UpdateAsync(Arg.Any<Sale>()).Returns(Task.CompletedTask);

            // When
            var result = await _handler.Handle(command, CancellationToken.None);

            // Then
            result.Should().Be(MediatR.Unit.Value);

            await _saleRepository.Received(1).GetByIdAsync(saleId);
            await _saleRepository.Received(1).UpdateAsync(Arg.Is<Sale>(s =>
                s.Id == saleId &&
                s.SaleItems.Count == initialItemCount - 1 &&
                !s.SaleItems.Any(item => item.Id == itemToRemoveId) &&
                s.TotalAmount < initialTotalAmount
            ));

            await _mediator.Received(1).Publish(Arg.Is<SaleModifiedEvent>(e => e.SaleId == saleId), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Tests that removing an item from a non-existent sale throws an InvalidOperationException.
        /// </summary>
        [Fact(DisplayName = "Given non-existent sale ID When removing item Then throws InvalidOperationException")]
        public async Task Handle_NonExistentSale_ThrowsInvalidOperationException()
        {
            // Given
            var saleId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var command = new RemoveItemFromSaleCommand { SaleId = saleId, ItemId = itemId };

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
        /// Tests that removing a non-existent item from an existing sale does not cause an error and does not modify the sale.
        /// </summary>
        [Fact(DisplayName = "Given non-existent item ID in sale When removing item Then sale is not modified")]
        public async Task Handle_NonExistentItemInSale_SaleNotModified()
        {
            // Given
            var saleId = Guid.NewGuid();
            var nonExistentItemId = Guid.NewGuid();
            var saleWithItems = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId, SaleStatus.Active);
            var initialItemCount = saleWithItems.SaleItems.Count;
            var initialTotalAmount = saleWithItems.TotalAmount;

            var command = new RemoveItemFromSaleCommand { SaleId = saleId, ItemId = nonExistentItemId };

            _saleRepository.GetByIdAsync(saleId).Returns(saleWithItems);

            // When
            var result = await _handler.Handle(command, CancellationToken.None);

            // Then
            result.Should().Be(MediatR.Unit.Value);

            await _saleRepository.Received(1).GetByIdAsync(saleId);
            await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
            await _mediator.DidNotReceive().Publish(Arg.Any<SaleModifiedEvent>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Tests that removing an item from a cancelled sale throws an InvalidOperationException.
        /// </summary>
        [Fact(DisplayName = "Given cancelled sale When removing item Then throws InvalidOperationException")]
        public async Task Handle_CancelledSale_ThrowsInvalidOperationException()
        {
            // Given
            var saleId = Guid.NewGuid();
            var existingItemId = Guid.NewGuid();
            var cancelledSale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId, SaleStatus.Active);
            var itemToPreExist = new SaleItem("PREEXIST-PROD", "Item Pré-existente", 1, 10.00m);
            itemToPreExist.GetType().GetProperty(nameof(SaleItem.Id))?.SetValue(itemToPreExist, existingItemId);
            cancelledSale.AddItem(itemToPreExist);
            cancelledSale.CancelSale();

            var command = new RemoveItemFromSaleCommand { SaleId = saleId, ItemId = existingItemId };

            _saleRepository.GetByIdAsync(saleId).Returns(cancelledSale);

            // When
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Then
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot remove items from a cancelled sale.");

            await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
            await _mediator.DidNotReceive().Publish(Arg.Any<SaleModifiedEvent>(), Arg.Any<CancellationToken>());
        }
    }
}