using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpolyCafe.Infrastructure.Persistence.Configurations;

public class InventoryReceiptDetailConfiguration : IEntityTypeConfiguration<InventoryReceiptDetail>
{
    public void Configure(EntityTypeBuilder<InventoryReceiptDetail> builder)
    {
        builder.HasKey(ird => new { ird.ReceiptId, ird.IngredientId });
        builder.Property(ird => ird.Quantity).HasColumnType("decimal(18,2)");
        builder.Property(ird => ird.UnitPrice).HasColumnType("decimal(18,2)");

        builder.HasOne(ird => ird.InventoryReceipt)
            .WithMany(ir => ir.InventoryReceiptDetails)
            .HasForeignKey(ird => ird.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ird => ird.Ingredient)
            .WithMany(i => i.InventoryReceiptDetails)
            .HasForeignKey(ird => ird.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
