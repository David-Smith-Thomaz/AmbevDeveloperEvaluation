using MediatR;

namespace Ambev.DeveloperEvaluation.Domain.Events
{
    public class SaleCreatedEvent : INotification
    {
        public Guid SaleId { get; }
        public DateTime CreatedAt { get; }

        public SaleCreatedEvent(Guid saleId)
        {
            SaleId = saleId;
            CreatedAt = DateTime.UtcNow;
        }
    }
}