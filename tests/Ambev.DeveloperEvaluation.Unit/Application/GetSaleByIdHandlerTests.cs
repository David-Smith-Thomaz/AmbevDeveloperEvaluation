using Ambev.DeveloperEvaluation.Application.Sales.Queries.GetSaleById;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using NSubstitute;
using Xunit;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales
{
    /// <summary>
    /// Contains unit tests for the <see cref="GetSaleByIdHandler"/> class.
    /// </summary>
    public class GetSaleByIdHandlerTests
    {
        private readonly ISaleRepository _saleRepository;
        private readonly GetSaleByIdHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetSaleByIdHandlerTests"/> class.
        /// Sets up the test dependencies.
        /// </summary>
        public GetSaleByIdHandlerTests()
        {
            _saleRepository = Substitute.For<ISaleRepository>();
            _handler = new GetSaleByIdHandler(_saleRepository);
        }

        /// <summary>
        /// Tests that a valid query returns the expected sale entity.
        /// </summary>
        [Fact(DisplayName = "Given valid sale ID When getting sale by ID Then returns sale entity")]
        public async Task Handle_ValidId_ReturnsSaleEntity()
        {
            // Given
            var saleId = Guid.NewGuid();
            var query = new GetSaleByIdQuery { Id = saleId };
            var expectedSale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId);

            _saleRepository.GetByIdAsync(saleId).Returns(expectedSale);

            // When
            var result = await _handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedSale);
            await _saleRepository.Received(1).GetByIdAsync(saleId);
        }

        /// <summary>
        /// Tests that a query for a non-existent sale returns null.
        /// </summary>
        [Fact(DisplayName = "Given non-existent sale ID When getting sale by ID Then returns null")]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            // Given
            var saleId = Guid.NewGuid();
            var query = new GetSaleByIdQuery { Id = saleId };
            _saleRepository.GetByIdAsync(saleId).Returns((Sale?)null);

            // When
            var result = await _handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().BeNull();
            await _saleRepository.Received(1).GetByIdAsync(saleId);
        }
    }
}