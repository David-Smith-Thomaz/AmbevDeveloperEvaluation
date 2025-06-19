using Ambev.DeveloperEvaluation.Application.Sales.Queries.GetSaleItemById;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using NSubstitute;
using Xunit;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales
{
    /// <summary>
    /// Contains unit tests for the <see cref="GetSaleItemByIdHandler"/> class.
    /// </summary>
    public class GetSaleItemByIdHandlerTests
    {
        private readonly ISaleRepository _saleRepository;
        private readonly GetSaleItemByIdHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSaleItemByIdHandlerTests"/> class.
        /// Sets up the test dependencies.
        /// </summary>
        public GetSaleItemByIdHandlerTests()
        {
            _saleRepository = Substitute.For<ISaleRepository>();
            _handler = new GetSaleItemByIdHandler(_saleRepository);
        }

        /// <summary>
        /// Tests that a valid query returns the expected sale item entity.
        /// </summary>
        [Fact(DisplayName = "Given valid sale and item IDs When getting sale item by ID Then returns sale item entity")]
        public async Task Handle_ValidSaleAndItemIds_ReturnsSaleItemEntity()
        {
            // Given
            var saleId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var expectedSaleItem = new SaleItem("PROD-GET-001", "Item a ser buscado", 1, 10.00m);
            expectedSaleItem.GetType().GetProperty(nameof(SaleItem.Id))?.SetValue(expectedSaleItem, itemId);

            var saleWithItem = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId);
            saleWithItem.AddItem(expectedSaleItem);

            var query = new GetSaleItemByIdQuery { SaleId = saleId, ItemId = itemId };

            _saleRepository.GetByIdAsync(saleId).Returns(saleWithItem);

            // When
            var result = await _handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedSaleItem);
            await _saleRepository.Received(1).GetByIdAsync(saleId);
        }

        /// <summary>
        /// Tests that a query for a non-existent sale returns null.
        /// </summary>
        [Fact(DisplayName = "Given non-existent sale ID When getting sale item by ID Then returns null")]
        public async Task Handle_NonExistentSale_ReturnsNull()
        {
            // Given
            var saleId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var query = new GetSaleItemByIdQuery { SaleId = saleId, ItemId = itemId };

            _saleRepository.GetByIdAsync(saleId).Returns((Sale?)null);

            // When
            var result = await _handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().BeNull();
            await _saleRepository.Received(1).GetByIdAsync(saleId);
        }

        /// <summary>
        /// Tests that a query for a non-existent item within an existing sale returns null.
        /// </summary>
        [Fact(DisplayName = "Given non-existent item ID in existing sale When getting sale item by ID Then returns null")]
        public async Task Handle_NonExistentItemInSale_ReturnsNull()
        {
            // Given
            var saleId = Guid.NewGuid();
            var nonExistentItemId = Guid.NewGuid();
            var saleWithoutSpecificItem = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId);

            var query = new GetSaleItemByIdQuery { SaleId = saleId, ItemId = nonExistentItemId };

            _saleRepository.GetByIdAsync(saleId).Returns(saleWithoutSpecificItem);

            // When
            var result = await _handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().BeNull();
            await _saleRepository.Received(1).GetByIdAsync(saleId);
        }
    }
}