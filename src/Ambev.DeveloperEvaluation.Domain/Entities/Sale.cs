using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Validation;

namespace Ambev.DeveloperEvaluation.Domain.Entities
{
    public class Sale : BaseEntity
    {
        public string SaleNumber { get; private set; }
        public DateTime SaleDate { get; private set; }
        public string CustomerId { get; private set; }
        public string CustomerName { get; private set; }
        public string BranchId { get; private set; }
        public string BranchName { get; private set; }
        public decimal TotalAmount { get; private set; }
        public SaleStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public byte[] RowVersion { get; private set; }

        private readonly List<SaleItem> _saleItems = new List<SaleItem>();
        public IReadOnlyCollection<SaleItem> SaleItems => _saleItems.AsReadOnly();

        private Sale() { }

        public Sale(string saleNumber, DateTime saleDate, string customerId, string customerName, string branchId, string branchName)
        {
            Id = Guid.NewGuid();
            SaleNumber = saleNumber;
            SaleDate = saleDate;
            CustomerId = customerId;
            CustomerName = customerName;
            BranchId = branchId;
            BranchName = branchName;
            Status = SaleStatus.Active;
            CreatedAt = DateTime.UtcNow;
            TotalAmount = 0;
        }

        public void AddItem(SaleItem item)
        {
            if (Status == SaleStatus.Cancelled)
            {
                throw new InvalidOperationException("Cannot add items to a cancelled sale.");
            }
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "Sale item cannot be null.");
            }
            item.SetSaleId(Id);
            _saleItems.Add(item);
            RecalculateTotalAmount();
            UpdatedAt = DateTime.UtcNow;
        }

        public bool RemoveItem(Guid itemId)
        {
            var itemToRemove = _saleItems.FirstOrDefault(i => i.Id == itemId);
            if (itemToRemove != null)
            {
                _saleItems.Remove(itemToRemove);
                RecalculateTotalAmount();
                UpdatedAt = DateTime.UtcNow;
                return true;
            }
            return false;
        }

        public void CancelSale()
        {
            if (Status == SaleStatus.Cancelled)
            {
                throw new InvalidOperationException("Sale is already cancelled.");
            }
            Status = SaleStatus.Cancelled;
            foreach (var item in _saleItems)
            {
                item.CancelItem();
            }
            RecalculateTotalAmount();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateSaleDetails(string customerName, string branchName)
        {
            if (Status == SaleStatus.Cancelled)
            {
                throw new InvalidOperationException("Cannot update details of a cancelled sale.");
            }
            CustomerName = customerName;
            BranchName = branchName;
            UpdatedAt = DateTime.UtcNow;
        }

        private void RecalculateTotalAmount()
        {
            TotalAmount = _saleItems.Where(item => !item.IsCancelled).Sum(item => item.TotalAmount);
        }

        public new async Task<IEnumerable<ValidationErrorDetail>> ValidateAsync()
        {
            var validator = new SaleValidator();
            var result = await validator.ValidateAsync(this);

            if (!result.IsValid)
            {
                var validationErrorDetails = result.Errors.Select(failure => (ValidationErrorDetail)failure).ToList();

                throw new DomainValidationException("One or more domain validation errors occurred.", validationErrorDetails);
            }

            return Enumerable.Empty<ValidationErrorDetail>();
        }
    }
}