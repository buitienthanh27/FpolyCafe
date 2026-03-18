using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpolyCafe.Infrastructure.Persistence.Configurations;

public class BillDetailConfiguration : IEntityTypeConfiguration<BillDetail>
{
    public void Configure(EntityTypeBuilder<BillDetail> builder)
    {
        builder.HasKey(bd => bd.BillDetailId);
        builder.Property(bd => bd.HistoricalProductName).IsRequired().HasMaxLength(200);
        builder.Property(bd => bd.HistoricalPrice).HasColumnType("decimal(18,2)");
        builder.Property(bd => bd.SizeName).IsRequired().HasMaxLength(10);
        builder.Property(bd => bd.Notes).HasMaxLength(500);

        builder.HasOne(bd => bd.Bill)
            .WithMany(b => b.BillDetails)
            .HasForeignKey(bd => bd.BillId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bd => bd.Product)
            .WithMany(p => p.BillDetails)
            .HasForeignKey(bd => bd.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bd => bd.Size)
            .WithMany()
            .HasForeignKey(bd => bd.SizeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
