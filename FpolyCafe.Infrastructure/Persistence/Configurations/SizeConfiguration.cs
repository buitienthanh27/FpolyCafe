using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpolyCafe.Infrastructure.Persistence.Configurations;

public class SizeConfiguration : IEntityTypeConfiguration<Size>
{
    public void Configure(EntityTypeBuilder<Size> builder)
    {
        builder.HasKey(s => s.SizeId);
        builder.Property(s => s.SizeName).IsRequired().HasMaxLength(10);
    }
}
