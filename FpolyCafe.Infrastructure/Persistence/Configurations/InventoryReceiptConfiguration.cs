using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpolyCafe.Infrastructure.Persistence.Configurations;

public class InventoryReceiptConfiguration : IEntityTypeConfiguration<InventoryReceipt>
{
    public void Configure(EntityTypeBuilder<InventoryReceipt> builder)
    {
        builder.HasKey(ir => ir.ReceiptId);
        builder.Property(ir => ir.TotalCost).HasColumnType("decimal(18,2)");
        builder.Property(ir => ir.Supplier).HasMaxLength(200);
        builder.Property(ir => ir.Notes).HasMaxLength(500);

        builder.HasOne(ir => ir.User)
            .WithMany(u => u.InventoryReceipts)
            .HasForeignKey(ir => ir.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
