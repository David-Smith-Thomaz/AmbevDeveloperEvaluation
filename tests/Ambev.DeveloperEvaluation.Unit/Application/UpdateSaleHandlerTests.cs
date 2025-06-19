using Ambev.DeveloperEvaluation.Application.Sales.Commands.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
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
    /// Contains unit tests for the <see cref="UpdateSaleHandler"/> class.
    /// </summary>
    public class UpdateSaleHandlerTests
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMediator _mediator;
        private readonly UpdateSaleHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateSaleHandlerTests"/> class.
        /// Sets up the test dependencies.
        /// </summary>
        public UpdateSaleHandlerTests()
        {
            _saleRepository = Substitute.For<ISaleRepository>();
            _mediator = Substitute.For<IMediator>();
            _handler = new UpdateSaleHandler(_saleRepository, _mediator);
        }

        /// <summary>
        /// Tests that a valid update sale request is handled successfully.
        /// </summary>
        [Fact(DisplayName = "Given valid sale data When updating sale Then returns Unit.Value and publishes event")]
        public async Task Handle_ValidRequest_ReturnsUnitValueAndPublishesEvent()
        {
            // Given
            var saleId = Guid.NewGuid();
            var command = UpdateSaleHandlerTestData.GenerateValidCommand(saleId);
            var existingSale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId);

            _saleRepository.GetByIdAsync(saleId).Returns(existingSale);
            _saleRepository.UpdateAsync(Arg.Any<Sale>()).Returns(Task.CompletedTask);

            // When
            var result = await _handler.Handle(command, CancellationToken.None);

            // Then
            result.Should().Be(MediatR.Unit.Value);

            await _saleRepository.Received(1).GetByIdAsync(saleId);
            await _saleRepository.Received(1).UpdateAsync(Arg.Is<Sale>(s =>
                s.Id == saleId &&
                s.CustomerName == command.CustomerName &&
                s.BranchName == command.BranchName
            ));

            await _mediator.Received(1).Publish(Arg.Is<SaleModifiedEvent>(e => e.SaleId == saleId), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Tests that updating a non-existent sale throws an InvalidOperationException.
        /// </summary>
        [Fact(DisplayName = "Given non-existent sale ID When updating sale Then throws InvalidOperationException")]
        public async Task Handle_NonExistentSale_ThrowsInvalidOperationException()
        {
            // Given
            var saleId = Guid.NewGuid();
            var command = UpdateSaleHandlerTestData.GenerateValidCommand(saleId);
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
        /// Tests that updating a cancelled sale throws an InvalidOperationException.
        /// </summary>
        [Fact(DisplayName = "Given cancelled sale When updating sale Then throws InvalidOperationException")]
        public async Task Handle_CancelledSale_ThrowsInvalidOperationException()
        {
            var saleId = Guid.NewGuid();
            var command = UpdateSaleHandlerTestData.GenerateValidCommand(saleId);
            var cancelledSale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId, Ambev.DeveloperEvaluation.Domain.Enums.SaleStatus.Cancelled);

            _saleRepository.GetByIdAsync(saleId).Returns(cancelledSale);

            // When
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Then
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Cannot update details of a cancelled sale.");

            await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
            await _mediator.DidNotReceive().Publish(Arg.Any<SaleModifiedEvent>(), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Tests that an invalid update sale request throws a domain validation exception.
        /// </summary>
        [Fact(DisplayName = "Given invalid update data When updating sale Then throws domain validation exception")]
        public async Task Handle_InvalidUpdateData_ThrowsDomainValidationException()
        {
            // Given
            var saleId = Guid.NewGuid();
            var command = UpdateSaleHandlerTestData.GenerateInvalidCommand(saleId);
            var existingSale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId);

            _saleRepository.GetByIdAsync(saleId).Returns(existingSale);

            // When
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Then
            await act.Should().ThrowAsync<DomainValidationException>()
                .WithMessage("One or more domain validation errors occurred.");

            await _saleRepository.DidNotReceive().UpdateAsync(Arg.Any<Sale>());
            await _mediator.DidNotReceive().Publish(Arg.Any<SaleModifiedEvent>(), Arg.Any<CancellationToken>());
        }
    }
}