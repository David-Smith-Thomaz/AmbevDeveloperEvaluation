using Ambev.DeveloperEvaluation.Application.Sales.Commands.CreateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData
{
    public static class CreateSaleHandlerTestData
    {
        private static readonly Faker<CreateSaleCommand.CreateSaleCommandItem> createSaleCommandItemFaker = new Faker<CreateSaleCommand.CreateSaleCommandItem>()
            .RuleFor(i => i.ProductId, f => f.Commerce.ProductAdjective() + " " + f.Commerce.Product())
            .RuleFor(i => i.ProductName, f => f.Commerce.ProductName())
            .RuleFor(i => i.Quantity, f => f.Random.Int(1, 20))
            .RuleFor(i => i.UnitPrice, f => f.Finance.Amount(0.01m, 1000.00m));

        private static readonly Faker<CreateSaleCommand> createSaleCommandFaker = new Faker<CreateSaleCommand>()
            .RuleFor(c => c.SaleNumber, f => f.Commerce.Ean13())
            .RuleFor(c => c.SaleDate, f => f.Date.Past(1))
            .RuleFor(c => c.CustomerId, f => f.Finance.RoutingNumber())
            .RuleFor(c => c.CustomerName, f => f.Company.CompanyName())
            .RuleFor(c => c.BranchId, f => f.Random.AlphaNumeric(8))
            .RuleFor(c => c.BranchName, f => f.Company.CatchPhrase())
            .RuleFor(c => c.Items, f => Enumerable.Range(1, f.Random.Int(1, 3))
                                                  .Select(_ => createSaleCommandItemFaker.Generate())
                                                  .ToList());

        public static CreateSaleCommand GenerateValidCommand()
        {
            return createSaleCommandFaker.Generate();
        }

        public static CreateSaleCommand GenerateInvalidCommand()
        {
            return new CreateSaleCommand
            {
                SaleNumber = null!,
                SaleDate = DateTime.UtcNow.AddDays(1),
                Items = new List<CreateSaleCommand.CreateSaleCommandItem>()
            };
        }

        public static CreateSaleCommand GenerateCommandWithInvalidItemQuantity()
        {
            var command = GenerateValidCommand();
            command.Items.Add(new CreateSaleCommand.CreateSaleCommandItem
            {
                ProductId = "INVALID-QTY-PROD",
                ProductName = "Produto com Qtd Inválida",
                Quantity = 21,
                UnitPrice = 1.00m
            });
            return command;
        }

        public static CreateSaleCommand GenerateCommandWithSpecificItemQuantities(List<int> itemQuantities)
        {
            var command = GenerateValidCommand();
            command.Items.Clear();

            foreach (var qty in itemQuantities)
            {
                command.Items.Add(new CreateSaleCommand.CreateSaleCommandItem
                {
                    ProductId = Guid.NewGuid().ToString(),
                    ProductName = $"Test Item Qty {qty}",
                    Quantity = qty,
                    UnitPrice = 100.00m
                });
            }
            return command;
        }
    }
}