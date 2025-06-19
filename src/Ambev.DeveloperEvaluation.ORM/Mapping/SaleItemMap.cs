using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.ORM.Mapping
{
    public class SaleItemMap : IEntityTypeConfiguration<SaleItem>
    {
        public void Configure(EntityTypeBuilder<SaleItem> builder)
        {
            builder.HasKey(si => si.Id);
            builder.Property(si => si.ProductId).IsRequired().HasMaxLength(100);
            builder.Property(si => si.ProductName).IsRequired().HasMaxLength(200);
            builder.Property(si => si.Quantity).IsRequired();
            builder.Property(si => si.UnitPrice).HasColumnType("numeric(18,2)").IsRequired();
            builder.Property(si => si.Discount).HasColumnType("numeric(18,2)").IsRequired();
            builder.Property(si => si.TotalAmount).HasColumnType("numeric(18,2)").IsRequired();
            builder.Property(si => si.IsCancelled).IsRequired();
            builder.Property(si => si.CreatedAt).IsRequired();
            builder.Property(si => si.UpdatedAt).IsRequired(false);
            builder.Property(si => si.RowVersion).IsRowVersion();
        }
    }
}