using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpolyCafe.Infrastructure.Persistence.Configurations;

public class ToppingConfiguration : IEntityTypeConfiguration<Topping>
{
    public void Configure(EntityTypeBuilder<Topping> builder)
    {
        builder.HasKey(t => t.ToppingId);
        builder.Property(t => t.ToppingName).IsRequired().HasMaxLength(150);
        builder.Property(t => t.Price).HasColumnType("decimal(18,2)");
        builder.Property(t => t.QuantityNeeded).HasColumnType("decimal(18,2)");

        builder.HasOne(t => t.Ingredient)
            .WithMany()
            .HasForeignKey(t => t.IngredientId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
