using Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale;
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
    /// Contains unit tests for the <see cref="CreateSaleHandler"/> class.
    /// </summary>
    public class CreateSaleHandlerTests
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMediator _mediator;
        private readonly CreateSaleHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateSaleHandlerTests"/> class.
        /// Sets up the test dependencies.
        /// </summary>
        public CreateSaleHandlerTests()
        {
            _saleRepository = Substitute.For<ISaleRepository>();
            _mediator = Substitute.For<IMediator>();
            _handler = new CreateSaleHandler(_saleRepository, _mediator);
        }

        /// <summary>
        /// Tests that a valid sale creation request is handled successfully.
        /// </summary>
        [Fact(DisplayName = "Given valid sale data When creating sale Then returns sale ID and publishes event")]
        public async Task Handle_ValidRequest_ReturnsSaleIdAndPublishesEvent()
        {
            // Given
            var command = CreateSaleHandlerTestData.GenerateValidCommand();
            _saleRepository.AddAsync(Arg.Any<Sale>()).Returns(Task.CompletedTask);

            // When
            var createdSaleId = await _handler.Handle(command, CancellationToken.None);

            // Then
            createdSaleId.Should().NotBeEmpty();

            await _saleRepository.Received(1).AddAsync(Arg.Is<Sale>(sale =>
                sale.SaleNumber == command.SaleNumber &&
                sale.SaleItems.Count == command.Items.Count &&
                sale.TotalAmount > 0
            ));

            await _mediator.Received(1).Publish(Arg.Is<SaleCreatedEvent>(e => e.SaleId == createdSaleId), Arg.Any<CancellationToken>());
        }

        /// <summary>
        /// Tests that an invalid sale creation request throws a domain validation exception.
        /// (This test relies on the domain validation logic within the Sale entity)
        /// </summary>
        [Fact(DisplayName = "Given invalid sale data When creating sale Then throws domain validation exception")]
        public async Task Handle_InvalidRequest_ThrowsDomainValidationException()
        {
            // Given
            var command = CreateSaleHandlerTestData.GenerateInvalidCommand();

            // When
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Then
            await act.Should().ThrowAsync<DomainValidationException>()
                 .WithMessage("One or more domain validation errors occurred.");
        }

        /// <summary>
        /// Tests that a sale creation request with an invalid item quantity throws a domain validation exception.
        /// </summary>
        [Fact(DisplayName = "Given invalid item quantity When creating sale Then throws domain validation exception")]
        public async Task Handle_InvalidItemQuantity_ThrowsDomainValidationException()
        {
            // Given
            var command = CreateSaleHandlerTestData.GenerateCommandWithInvalidItemQuantity();

            // When
            var act = async () => await _handler.Handle(command, CancellationToken.None);

            // Then
            var exception = await act.Should().ThrowAsync<DomainValidationException>();

            exception.Which.Errors.Should().NotBeEmpty();
            exception.Which.Errors.Should().Contain(e => e.Detail.Contains("Cannot sell more than 20 identical items."));
        }

        /// <summary>
        /// Tests that discount rules are applied correctly when creating a sale.
        /// (This test relies on the domain logic within SaleItem)
        /// </summary>
        [Fact(DisplayName = "Given sale data with varying quantities When creating sale Then discount rules are applied correctly")]
        public async Task Handle_ValidRequest_AppliesDiscountRulesCorrectly()
        {
            // Given
            var command = CreateSaleHandlerTestData.GenerateCommandWithSpecificItemQuantities(new List<int> { 2, 5, 12 });

            Sale capturedSale = null!;
            _saleRepository.AddAsync(Arg.Do<Sale>(sale => capturedSale = sale)).Returns(Task.CompletedTask);

            // When
            await _handler.Handle(command, CancellationToken.None);

            // Then
            capturedSale.Should().NotBeNull();
            capturedSale.SaleItems.Should().HaveCount(3);

            var item1 = capturedSale.SaleItems.FirstOrDefault(i => i.Quantity == 2);
            item1.Should().NotBeNull();
            item1!.Discount.Should().Be(0m);
            item1.TotalAmount.Should().Be(2 * 100.00m);

            var item2 = capturedSale.SaleItems.FirstOrDefault(i => i.Quantity == 5);
            item2.Should().NotBeNull();
            item2!.Discount.Should().Be(5 * 100.00m * 0.10m);
            item2.TotalAmount.Should().Be((5 * 100.00m) - (5 * 100.00m * 0.10m));

            var item3 = capturedSale.SaleItems.FirstOrDefault(i => i.Quantity == 12);
            item3.Should().NotBeNull();
            item3!.Discount.Should().Be(12 * 100.00m * 0.20m);
            item3.TotalAmount.Should().Be((12 * 100.00m) - (12 * 100.00m * 0.20m));

            capturedSale.TotalAmount.Should().Be(item1.TotalAmount + item2.TotalAmount + item3.TotalAmount);
        }
    }
}