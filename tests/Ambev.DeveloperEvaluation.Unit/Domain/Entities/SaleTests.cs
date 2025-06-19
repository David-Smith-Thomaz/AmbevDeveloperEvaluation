using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using Xunit;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities
{
    /// <summary>
    /// Contains unit tests for the <see cref="Sale"/> entity.
    /// Tests cover item manipulation, status changes, and validation scenarios.
    /// </summary>
    public class SaleTests
    {
        /// <summary>
        /// Tests that a sale is created with correct properties and default status.
        /// </summary>
        [Fact(DisplayName = "Given valid parameters When creating Sale Then properties are set correctly")]
        public void CreateSale_ValidParameters_PropertiesAreSet()
        {
            var saleNumber = "SALE-123";
            var saleDate = DateTime.UtcNow.AddHours(-1);
            var customerId = "CUST-001";
            var customerName = "Test Customer";
            var branchId = "BR-001";
            var branchName = "Test Branch";

            // Act
            var sale = new Sale(saleNumber, saleDate, customerId, customerName, branchId, branchName);

            // Assert
            sale.Should().NotBeNull();
            sale.Id.Should().NotBeEmpty();
            sale.SaleNumber.Should().Be(saleNumber);
            sale.SaleDate.Should().Be(saleDate);
            sale.CustomerId.Should().Be(customerId);
            sale.CustomerName.Should().Be(customerName);
            sale.BranchId.Should().Be(branchId);
            sale.BranchName.Should().Be(branchName);
            sale.Status.Should().Be(SaleStatus.Active);
            sale.TotalAmount.Should().Be(0m);
            sale.SaleItems.Should().BeEmpty();
        }

        /// <summary>
        /// Tests that adding a valid item updates the sale's properties correctly.
        /// </summary>
        [Fact(DisplayName = "Given active Sale When adding valid item Then item is added and TotalAmount updates")]
        public void AddItem_ValidItem_AddsItemAndUpdatesTotalAmount()
        {
            // Arrange
            var sale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(Guid.NewGuid(), SaleStatus.Active);
            var initialItemCount = sale.SaleItems.Count;
            var initialTotalAmount = sale.TotalAmount;

            var newItem = new SaleItem("PROD-ADD", "New Product", 5, 20.00m);

            // Act
            sale.AddItem(newItem);

            // Assert
            sale.SaleItems.Should().HaveCount(initialItemCount + 1);
            sale.SaleItems.Should().Contain(newItem);
            sale.TotalAmount.Should().Be(initialTotalAmount + newItem.TotalAmount);
        }

        /// <summary>
        /// Tests that attempting to add an item to a cancelled sale throws an exception.
        /// </summary>
        [Fact(DisplayName = "Given cancelled Sale When adding item Then throws InvalidOperationException")]
        public void AddItem_CancelledSale_ThrowsInvalidOperationException()
        {
            // Arrange
            var sale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(Guid.NewGuid(), SaleStatus.Cancelled);
            var newItem = new SaleItem("PROD-ADD", "New Product", 1, 10.00m);

            // Act
            Action act = () => sale.AddItem(newItem);

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Cannot add items to a cancelled sale.");
            sale.SaleItems.Should().NotContain(newItem);
        }

        /// <summary>
        /// Tests that removing an existing item updates the sale's properties.
        /// </summary>
        [Fact(DisplayName = "Given Sale with items When removing existing item Then item is removed and TotalAmount updates")]
        public void RemoveItem_ExistingItem_RemovesItemAndUpdatesTotalAmount()
        {
            // Arrange
            var saleId = Guid.NewGuid();
            var itemToRemoveId = Guid.NewGuid();
            var sale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(saleId, SaleStatus.Active);
            var itemToRemove = new SaleItem("PROD-REMOVE", "Product to Remove", 1, 50.00m);
            itemToRemove.GetType().GetProperty(nameof(SaleItem.Id))?.SetValue(itemToRemove, itemToRemoveId);
            sale.AddItem(itemToRemove);

            var initialTotalAmount = sale.TotalAmount;
            var initialItemCount = sale.SaleItems.Count;

            // Act
            var removed = sale.RemoveItem(itemToRemoveId);

            // Assert
            removed.Should().BeTrue();
            sale.SaleItems.Should().HaveCount(initialItemCount - 1);
            sale.SaleItems.Should().NotContain(itemToRemove);
            sale.TotalAmount.Should().Be(initialTotalAmount - itemToRemove.TotalAmount);
        }

        /// <summary>
        /// Tests that attempting to remove a non-existent item returns false and doesn't change the sale.
        /// </summary>
        [Fact(DisplayName = "Given Sale When removing non-existent item Then returns false and Sale is unchanged")]
        public void RemoveItem_NonExistentItem_ReturnsFalseAndSaleIsUnchanged()
        {
            // Arrange
            var sale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(Guid.NewGuid(), SaleStatus.Active);
            var nonExistentItemId = Guid.NewGuid();
            var initialItemCount = sale.SaleItems.Count;
            var initialTotalAmount = sale.TotalAmount;

            // Act
            var removed = sale.RemoveItem(nonExistentItemId);

            // Assert
            removed.Should().BeFalse();
            sale.SaleItems.Should().HaveCount(initialItemCount);
            sale.TotalAmount.Should().Be(initialTotalAmount);
        }

        /// <summary>
        /// Tests that cancelling an active sale correctly updates its status and total amount.
        /// </summary>
        [Fact(DisplayName = "Given active Sale When cancelling Then status is Cancelled and TotalAmount is zeroed")]
        public void CancelSale_ActiveSale_CancelsAndZeroesAmounts()
        {
            // Arrange
            var sale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(Guid.NewGuid(), SaleStatus.Active);

            // Act
            sale.CancelSale();

            // Assert
            sale.Status.Should().Be(SaleStatus.Cancelled); 
            sale.TotalAmount.Should().Be(0m);
            sale.SaleItems.Should().AllSatisfy(item => item.IsCancelled.Should().BeTrue());
        }

        /// <summary>
        /// Tests that attempting to cancel an already cancelled sale throws an exception.
        /// </summary>
        [Fact(DisplayName = "Given cancelled Sale When cancelling again Then throws InvalidOperationException")]
        public void CancelSale_CancelledSale_ThrowsInvalidOperationException()
        {
            // Arrange
            var sale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(Guid.NewGuid(), SaleStatus.Active);
            sale.CancelSale();

            // Act
            Action act = () => sale.CancelSale(); 

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Sale is already cancelled.");
        }

        /// <summary>
        /// Tests that updating sale details correctly updates the properties.
        /// </summary>
        [Fact(DisplayName = "Given active Sale When updating details Then CustomerName and BranchName are updated")]
        public void UpdateSaleDetails_UpdatesProperties()
        {
            // Arrange
            var sale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(Guid.NewGuid(), SaleStatus.Active);
            var newCustomerName = "New Customer Inc.";
            var newBranchName = "Main Branch Updated";

            // Act
            sale.UpdateSaleDetails(newCustomerName, newBranchName);

            // Assert
            sale.CustomerName.Should().Be(newCustomerName);
            sale.BranchName.Should().Be(newBranchName);
            sale.UpdatedAt.Should().NotBeNull(); 
        }

        /// <summary>
        /// Tests that updating details of a cancelled sale throws an exception.
        /// </summary>
        [Fact(DisplayName = "Given cancelled Sale When updating details Then throws InvalidOperationException")]
        public void UpdateSaleDetails_CancelledSale_ThrowsInvalidOperationException()
        {
            // Arrange
            var sale = UpdateSaleHandlerTestData.GenerateValidSaleEntity(Guid.NewGuid(), SaleStatus.Cancelled);
            var newCustomerName = "New Customer Inc.";
            var newBranchName = "Main Branch Updated";

            // Act
            Action act = () => sale.UpdateSaleDetails(newCustomerName, newBranchName);

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("Cannot update details of a cancelled sale.");
        }
    }
}