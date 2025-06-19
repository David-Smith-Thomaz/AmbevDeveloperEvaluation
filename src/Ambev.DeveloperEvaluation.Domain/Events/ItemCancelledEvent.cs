using MediatR;
using System;

namespace Ambev.DeveloperEvaluation.Domain.Events
{
    public class ItemCancelledEvent : INotification
    {
        public Guid SaleId { get; }
        public Guid ItemId { get; }
        public DateTime CancelledAt { get; }

        public ItemCancelledEvent(Guid saleId, Guid itemId)
        {
            SaleId = saleId;
            ItemId = itemId;
            CancelledAt = DateTime.UtcNow;
        }
    }
}