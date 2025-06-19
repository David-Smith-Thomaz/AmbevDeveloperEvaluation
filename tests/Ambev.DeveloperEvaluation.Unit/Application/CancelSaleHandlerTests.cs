using Ambev.DeveloperEvaluation.Application.Sales.Commands.CancelSale;
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
    /// Contains unit tests for the <see cref="CancelSaleHandler"/> class.
    /// </summary>
    public class CancelSaleHandlerTests
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMediator _mediator;
        private readonly CancelSaleHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="CancelSaleHandlerTests"/> class.
        /// Sets up the test dependencies.
        /// </summary>
        public CancelSaleHandlerTests()
        {
            _saleRepository = Substitute.For<ISaleRepository>();
            _mediator = Substitute.For<IMediator>();
            _handler = new CancelSaleHandler(_saleRepository, _mediator);
        }

        /// <summary>
        /// Tests that a valid cancel sale request is handled successfully.
        /// </summary>
        [Fact(DisplayName = "Given active sale When canceling sale Then sale is cancelled and event is published")]
        public async Task Handle_ActiveSale_CancelsSaleAndPublishesEvent()
        {
            // Given
            var saleId = Guid.NewGuid();
            var command = new CancelSaleCommand { Id = saleId };
            var activeSale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId, SaleStatus.Active); // Generates an active sale

            _saleRepository.GetByIdAsync(saleId).Returns(activeSale);
            _saleRepository.UpdateAsync(Arg.Any<Sale>()).Returns(Task.CompletedTask);

            // When
            var result = await _handler.Handle(command, CancellationToken.None);

            // Then
            result.Should().Be(MediatR.Unit.Value);

            await _saleRepository.Received(1).GetByIdAsync(saleId);
            await _saleRepository.Received(1).UpdateAsync(Arg.Is<Sale>(s =>
                s.Id == saleId &&
                s.Status == SaleStatus.Cancelled &&
                s.TotalAmount == 0m
            ));

            await _mediator.Received(1).Publish(Arg.Is<SaleCancelledEvent>(e => e.SaleId == saleId), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Tests that canceling a non-existent sale throws an InvalidOperationException.
        /// </summary>
        [Fact(DisplayName = "Given non-existent sale ID When canceling sale Then throws InvalidOperationException")]
        public async Task Handle_NonExistentSale_ThrowsInvalidOperationException()
        {
            // Given
            var saleId = Guid.NewGuid();
            var command = new CancelSaleCommand { Id = saleId };
            _saleRepository.GetByIdAsync(saleId).Returns((Sale?)null);

            // When
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Then
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Sale with ID {saleId} not found.");

            await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
            await _mediator.DidNotReceive().Publish(Arg.Any<SaleCancelledEvent>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Tests that canceling an already cancelled sale throws an InvalidOperationException.
        /// </summary>
        [Fact(DisplayName = "Given already cancelled sale When canceling sale Then throws InvalidOperationException")]
        public async Task Handle_AlreadyCancelledSale_ThrowsInvalidOperationException()
        {
            // Given
            var saleId = Guid.NewGuid();
            var command = new CancelSaleCommand { Id = saleId };
            var cancelledSale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId, SaleStatus.Cancelled); // Generates an already cancelled sale

            _saleRepository.GetByIdAsync(saleId).Returns(cancelledSale);

            // When
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Then
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Sale is already cancelled.");

            await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
            await _mediator.DidNotReceive().Publish(Arg.Any<SaleCancelledEvent>(), Arg.Any<CancellationToken>());
        }
    }
}