using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpolyCafe.Infrastructure.Persistence.Configurations;

public class BillDetailToppingConfiguration : IEntityTypeConfiguration<BillDetailTopping>
{
    public void Configure(EntityTypeBuilder<BillDetailTopping> builder)
    {
        builder.HasKey(bdt => new { bdt.BillDetailId, bdt.ToppingId });
        builder.Property(bdt => bdt.HistoricalToppingName).IsRequired().HasMaxLength(150);
        builder.Property(bdt => bdt.HistoricalToppingPrice).HasColumnType("decimal(18,2)");

        builder.HasOne(bdt => bdt.BillDetail)
            .WithMany(bd => bd.BillDetailToppings)
            .HasForeignKey(bdt => bdt.BillDetailId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bdt => bdt.Topping)
            .WithMany(t => t.BillDetailToppings)
            .HasForeignKey(bdt => bdt.ToppingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
