using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpolyCafe.Infrastructure.Persistence.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.HasKey(r => new { r.ProductId, r.SizeId, r.IngredientId });
        builder.Property(r => r.QuantityNeeded).HasColumnType("decimal(18,2)");

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Recipes)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Size)
            .WithMany()
            .HasForeignKey(r => r.SizeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Ingredient)
            .WithMany(i => i.Recipes)
            .HasForeignKey(r => r.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
