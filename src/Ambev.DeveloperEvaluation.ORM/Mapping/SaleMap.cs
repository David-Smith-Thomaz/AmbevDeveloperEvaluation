using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.ORM.Mapping
{
    public class SaleMap : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.SaleNumber).IsRequired().HasMaxLength(50);
            builder.Property(s => s.SaleDate).IsRequired();
            builder.Property(s => s.CustomerId).IsRequired().HasMaxLength(100);
            builder.Property(s => s.CustomerName).IsRequired().HasMaxLength(200);
            builder.Property(s => s.BranchId).IsRequired().HasMaxLength(100);
            builder.Property(s => s.BranchName).IsRequired().HasMaxLength(200);
            builder.Property(s => s.TotalAmount).HasColumnType("numeric(18,2)").IsRequired();
            builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(s => s.CreatedAt).IsRequired();
            builder.Property(s => s.UpdatedAt).IsRequired(false);
            builder.Property(s => s.RowVersion).IsRowVersion();

            builder.HasMany(s => s.SaleItems)
                   .WithOne()
                   .HasForeignKey("SaleId")
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}