using Ambev.DeveloperEvaluation.Application.Sales.Queries.ListSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using NSubstitute;
using Xunit;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales
{
    /// <summary>
    /// Contains unit tests for the <see cref="ListSalesHandler"/> class.
    /// </summary>
    public class ListSalesHandlerTests
    {
        private readonly ISaleRepository _saleRepository;
        private readonly ListSalesHandler _handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListSalesHandlerTests"/> class.
        /// Sets up the test dependencies.
        /// </summary>
        public ListSalesHandlerTests()
        {
            _saleRepository = Substitute.For<ISaleRepository>();
            _handler = new ListSalesHandler(_saleRepository);
        }

        /// <summary>
        /// Tests that a query with default parameters returns a list of sales.
        /// </summary>
        [Fact(DisplayName = "Given default parameters When listing sales Then returns a list of sales")]
        public async Task Handle_DefaultParameters_ReturnsListOfSales()
        {
            // Given
            var query = new ListSalesQuery();
            var expectedSales = new List<Sale>
            {
                UpdateSaleHandlerTestData.GenerateValidSaleEntity(Guid.NewGuid()),
                UpdateSaleHandlerTestData.GenerateValidSaleEntity(Guid.NewGuid())
            };
            _saleRepository.ListAsync(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>()
            ).Returns(expectedSales);

            // When
            var result = await _handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedSales);
            await _saleRepository.Received(1).ListAsync(1, 10, null, null);
        }

        /// <summary>
        /// Tests that a query with custom pagination parameters returns a paginated list of sales.
        /// </summary>
        [Fact(DisplayName = "Given custom pagination parameters When listing sales Then returns paginated list")]
        public async Task Handle_CustomPagination_ReturnsPaginatedList()
        {
            // Given
            var query = new ListSalesQuery { PageNumber = 2, PageSize = 5 };
            var expectedSales = new List<Sale>
            {
                UpdateSaleHandlerTestData.GenerateValidSaleEntity(Guid.NewGuid())
            };
            _saleRepository.ListAsync(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>()
            ).Returns(expectedSales);

            // When
            var result = await _handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedSales);
            await _saleRepository.Received(1).ListAsync(2, 5, null, null);
        }

        /// <summary>
        /// Tests that a query with ordering parameters correctly passes them to the repository.
        /// </summary>
        [Fact(DisplayName = "Given ordering parameters When listing sales Then passes ordering to repository")]
        public async Task Handle_OrderingParameters_PassesToRepository()
        {
            // Given
            var query = new ListSalesQuery { OrderBy = "SaleDate desc" };
            var expectedSales = new List<Sale>
            {
                UpdateSaleHandlerTestData.GenerateValidSaleEntity(Guid.NewGuid())
            };
            _saleRepository.ListAsync(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>()
            ).Returns(expectedSales);

            // When
            var result = await _handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedSales);
            await _saleRepository.Received(1).ListAsync(1, 10, "SaleDate desc", null);
        }

        /// <summary>
        /// Tests that a query with filtering parameters correctly passes them to the repository.
        /// </summary>
        [Fact(DisplayName = "Given filtering parameters When listing sales Then passes filtering to repository")]
        public async Task Handle_FilteringParameters_PassesToRepository()
        {
            // Given
            var query = new ListSalesQuery { Filter = "CustomerName=Test*" };
            var expectedSales = new List<Sale>
            {
                UpdateSaleHandlerTestData.GenerateValidSaleEntity(Guid.NewGuid())
            };
            _saleRepository.ListAsync(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>()
            ).Returns(expectedSales);

            // When
            var result = await _handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedSales);
            await _saleRepository.Received(1).ListAsync(1, 10, null, "CustomerName=Test*");
        }

        /// <summary>
        /// Tests that no sales are returned when the repository returns an empty list.
        /// </summary>
        [Fact(DisplayName = "Given no sales in repository When listing sales Then returns empty list")]
        public async Task Handle_NoSales_ReturnsEmptyList()
        {
            // Given
            var query = new ListSalesQuery();
            _saleRepository.ListAsync(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>()
            ).Returns(new List<Sale>());

            // When
            var result = await _handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull().And.BeEmpty();
            await _saleRepository.Received(1).ListAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string?>());
        }
    }
}