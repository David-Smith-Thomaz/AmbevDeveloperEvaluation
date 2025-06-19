using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Xunit;
using FluentAssertions;
using Ambev.DeveloperEvaluation.Common.Validation;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation
{
    /// <summary>
    /// Contains unit tests for the <see cref="SaleItemValidator"/> class.
    /// </summary>
    public class SaleItemValidatorTests
    {
        private readonly SaleItemValidator _validator;

        public SaleItemValidatorTests()
        {
            _validator = new SaleItemValidator();
        }

        /// <summary>
        /// Tests that a valid SaleItem entity passes validation.
        /// </summary>
        [Fact(DisplayName = "Given valid SaleItem entity When validating Then validation succeeds")]
        public async Task Validate_ValidSaleItem_Succeeds()
        {
            // Given
            var item = new SaleItem("PROD-X", "Product X", 5, 10.00m);

            // When
            var result = await _validator.ValidateAsync(item);

            // Then
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that a SaleItem with missing ProductId fails validation.
        /// </summary>
        [Fact(DisplayName = "Given SaleItem with missing ProductId When validating Then validation fails")]
        public async Task Validate_MissingProductId_Fails()
        {
            // Given
            var item = new SaleItem("", "Product X", 5, 10.00m);

            // When
            var result = await _validator.ValidateAsync(item);

            // Then
            result.IsValid.Should().BeFalse();
            var errorDetails = result.Errors.Select(f => (ValidationErrorDetail)f).ToList();
            errorDetails.Should().Contain(e => e.Error == "NotEmptyValidator" && e.Detail.Contains("Product ID cannot be empty."));
        }

        /// <summary>
        /// Tests that a SaleItem with invalid unit price fails validation.
        /// </summary>
        [Fact(DisplayName = "Given SaleItem with invalid UnitPrice When validating Then validation fails")]
        public async Task Validate_InvalidUnitPrice_Fails()
        {
            // Given
            var item = new SaleItem("PROD-X", "Product X", 1, -0.01m);

            // When
            var result = await _validator.ValidateAsync(item);

            // Then
            result.IsValid.Should().BeFalse();
            var errorDetails = result.Errors.Select(f => (ValidationErrorDetail)f).ToList();
            errorDetails.Should().Contain(e => e.Error == "GreaterThanOrEqualValidator" && e.Detail.Contains("Unit price must be a non-negative number."));
        }

        /// <summary>
        /// Tests that a SaleItem violating discount rules fails validation.
        /// </summary>
        [Fact(DisplayName = "Given SaleItem violating discount rules When validating Then validation fails")]
        public async Task Validate_ViolatingDiscountRules_Fails()
        {
            var item = new SaleItem("PROD-X", "Product X", 2, 10.00m);
            item.GetType().GetProperty(nameof(SaleItem.Discount))?.SetValue(item, 1.00m);

            // When
            var result = await _validator.ValidateAsync(item);

            // Then
            result.IsValid.Should().BeFalse();
            var errorDetails = result.Errors.Select(f => (ValidationErrorDetail)f).ToList();
            errorDetails.Should().Contain(e => e.Error == "PredicateValidator" && e.Detail.Contains("Discount rules violated for the item's quantity."));
        }
    }
}