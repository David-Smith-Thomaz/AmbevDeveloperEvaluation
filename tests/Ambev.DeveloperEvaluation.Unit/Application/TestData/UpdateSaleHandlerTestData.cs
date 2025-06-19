using Ambev.DeveloperEvaluation.Application.Sales.Commands.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData
{
    public static class UpdateSaleHandlerTestData
    {
        private static readonly Faker<UpdateSaleCommand> updateSaleCommandFaker = new Faker<UpdateSaleCommand>()
            .RuleFor(c => c.CustomerName, f => f.Company.CompanyName())
            .RuleFor(c => c.BranchName, f => f.Company.CatchPhrase());

        private static readonly Faker<SaleItem> saleItemEntityFaker = new Faker<SaleItem>()
            .CustomInstantiator(f => new SaleItem(
                f.Commerce.ProductAdjective() + f.Commerce.Product(),
                f.Commerce.ProductName(),
                f.Random.Int(1, 20),
                f.Finance.Amount(1.00m, 50.00m)
            ));

        public static UpdateSaleCommand GenerateValidCommand(Guid saleId)
        {
            var command = updateSaleCommandFaker.Generate();
            command.Id = saleId;
            return command;
        }

        public static UpdateSaleCommand GenerateInvalidCommand(Guid saleId)
        {
            return new UpdateSaleCommand
            {
                Id = saleId,
                CustomerName = null!,
                BranchName = null!
            };
        }

        /// <summary>
        /// Generates a valid Sale entity for testing purposes.
        /// This entity will now conditionally include items based on its status.
        /// </summary>
        public static Sale GenerateValidSaleEntity(Guid saleId, SaleStatus status = SaleStatus.Active)
        {
            var sale = new Sale(
                saleNumber: new Faker().Commerce.Ean13(),
                saleDate: new Faker().Date.Past(1),
                customerId: new Faker().Finance.RoutingNumber(),
                customerName: new Faker().Company.CompanyName(),
                branchId: new Faker().Random.AlphaNumeric(8),
                branchName: new Faker().Company.CatchPhrase()
            );

            sale.GetType().GetProperty(nameof(Sale.Id))?.SetValue(sale, saleId);
            sale.GetType().GetProperty(nameof(Sale.Status))?.SetValue(sale, status);

            if (status != SaleStatus.Cancelled)
            {
                var items = saleItemEntityFaker.Generate(new Faker().Random.Int(1, 3));
                foreach (var item in items)
                {
                    sale.AddItem(item);
                }
            }

            return sale;
        }
    }
}