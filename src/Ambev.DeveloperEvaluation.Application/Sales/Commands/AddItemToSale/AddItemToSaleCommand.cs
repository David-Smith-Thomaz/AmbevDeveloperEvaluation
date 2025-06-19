using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.Commands.AddItemToSale
{
    public class AddItemToSaleCommand : IRequest<Guid>
    {
        public Guid SaleId { get; set; }
        public AddItemToSaleCommandItem Item { get; set; } = new AddItemToSaleCommandItem();
        public record AddItemToSaleCommandItem
        {
            public string ProductId { get; set; } = string.Empty;
            public string ProductName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }
    }
}