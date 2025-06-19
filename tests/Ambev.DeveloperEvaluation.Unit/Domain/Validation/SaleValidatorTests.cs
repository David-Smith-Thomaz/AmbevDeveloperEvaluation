using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Xunit;
using FluentAssertions;
using Ambev.DeveloperEvaluation.Common.Validation;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation
{
    /// <summary>
    /// Contains unit tests for the <see cref="SaleValidator"/> class.
    /// </summary>
    public class SaleValidatorTests
    {
        private readonly SaleValidator _validator;

        public SaleValidatorTests()
        {
            _validator = new SaleValidator();
        }

        /// <summary>
        /// Tests that a valid Sale entity passes validation.
        /// </summary>
        [Fact(DisplayName = "Given valid Sale entity When validating Then validation succeeds")]
        public async Task Validate_ValidSale_Succeeds()
        {
            // Given
            var sale = new Sale(
                saleNumber: "TEST-001",
                saleDate: DateTime.UtcNow.AddMinutes(-5),
                customerId: "CUST-001",
                customerName: "Test Customer",
                branchId: "BR-001",
                branchName: "Test Branch"
            );
            sale.AddItem(new SaleItem("PROD-A", "Prod A", 1, 10.00m));

            // When
            var result = await _validator.ValidateAsync(sale);

            // Then
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that a Sale entity with missing SaleNumber fails validation.
        /// </summary>
        [Fact(DisplayName = "Given Sale with missing SaleNumber When validating Then validation fails")]
        public async Task Validate_MissingSaleNumber_Fails()
        {
            // Given
            var sale = new Sale(
                saleNumber: "",
                saleDate: DateTime.UtcNow.AddMinutes(-5),
                customerId: "CUST-001",
                customerName: "Test Customer",
                branchId: "BR-001",
                branchName: "Test Branch"
            );
            sale.AddItem(new SaleItem("PROD-A", "Prod A", 1, 10.00m));

            // When
            var result = await _validator.ValidateAsync(sale);

            // Then
            result.IsValid.Should().BeFalse();
            var errorDetails = result.Errors.Select(f => (ValidationErrorDetail)f).ToList();
            errorDetails.Should().Contain(e => e.Error == "NotEmptyValidator" && e.Detail.Contains("Sale number cannot be empty."));
        }

        /// <summary>
        /// Tests that a Sale entity with a future SaleDate fails validation.
        /// </summary>
        [Fact(DisplayName = "Given Sale with future SaleDate When validating Then validation fails")]
        public async Task Validate_FutureSaleDate_Fails()
        {
            // Given
            var sale = new Sale(
                saleNumber: "TEST-001",
                saleDate: DateTime.UtcNow.AddDays(1),
                customerId: "CUST-001",
                customerName: "Test Customer",
                branchId: "BR-001",
                branchName: "Test Branch"
            );
            sale.AddItem(new SaleItem("PROD-A", "Prod A", 1, 10.00m));

            // When
            var result = await _validator.ValidateAsync(sale);

            // Then
            result.IsValid.Should().BeFalse();
            var errorDetails = result.Errors.Select(f => (ValidationErrorDetail)f).ToList();
            errorDetails.Should().Contain(e => e.Error == "LessThanOrEqualValidator" && e.Detail.Contains("Sale date cannot be in the future."));
        }

        /// <summary>
        /// Tests that a Sale entity with an empty Items list fails validation.
        /// </summary>
        [Fact(DisplayName = "Given Sale with empty Items list When validating Then validation fails")]
        public async Task Validate_EmptyItemsList_Fails()
        {
            // Given
            var sale = new Sale(
                saleNumber: "TEST-001",
                saleDate: DateTime.UtcNow.AddMinutes(-5),
                customerId: "CUST-001",
                customerName: "Test Customer",
                branchId: "BR-001",
                branchName: "Test Branch"
            );
            var result = await _validator.ValidateAsync(sale);

            // Then
            result.IsValid.Should().BeFalse();
            var errorDetails = result.Errors.Select(f => (ValidationErrorDetail)f).ToList();
            errorDetails.Should().Contain(e => e.Error == "PredicateValidator" && e.Detail.Contains("A sale must have at least one item."));
        }
    }
}