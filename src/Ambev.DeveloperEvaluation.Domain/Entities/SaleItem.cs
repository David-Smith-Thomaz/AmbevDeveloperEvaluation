using System;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities
{
    public class SaleItem : BaseEntity
    {
        public Guid SaleId { get; private set; }
        public string ProductId { get; private set; }
        public string ProductName { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal Discount { get; private set; }
        public decimal TotalAmount { get; private set; }
        public bool IsCancelled { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public byte[] RowVersion { get; private set; }

        private SaleItem() { }

        public SaleItem(string productId, string productName, int quantity, decimal unitPrice)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            ProductName = productName;
            UnitPrice = unitPrice;
            IsCancelled = false;
            CreatedAt = DateTime.UtcNow;

            SetQuantity(quantity);
            CalculateTotalAmount();
        }

        public void SetSaleId(Guid saleId)
        {
            if (SaleId != Guid.Empty && SaleId != saleId)
            {
                throw new InvalidOperationException("SaleItem is already associated with a different Sale.");
            }
            SaleId = saleId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateItem(int newQuantity, decimal newUnitPrice)
        {
            if (IsCancelled)
            {
                throw new InvalidOperationException("Cannot update a cancelled item.");
            }
            SetQuantity(newQuantity);
            UnitPrice = newUnitPrice;
            CalculateTotalAmount();
            UpdatedAt = DateTime.UtcNow;
        }

        public void CancelItem()
        {
            if (IsCancelled)
            {
                throw new InvalidOperationException("Sale item is already cancelled.");
            }
            IsCancelled = true;
            Discount = 0;
            TotalAmount = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        private void SetQuantity(int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be a positive number.");
            }
            if (quantity > 20)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity), "Cannot sell more than 20 identical items.");
            }
            Quantity = quantity;
            CalculateDiscount();
        }

        private void CalculateDiscount()
        {
            if (Quantity >= 10 && Quantity <= 20)
            {
                Discount = (UnitPrice * Quantity) * 0.20m;
            }
            else if (Quantity >= 4)
            {
                Discount = (UnitPrice * Quantity) * 0.10m;
            }
            else
            {
                Discount = 0;
            }
        }

        private void CalculateTotalAmount()
        {
            TotalAmount = (UnitPrice * Quantity) - Discount;
        }

        public new async Task<IEnumerable<ValidationErrorDetail>> ValidateAsync()
        {
            var validator = new SaleItemValidator();
            var result = await validator.ValidateAsync(this);
            return result.Errors.Select(o => (ValidationErrorDetail)o);
        }
    }
}