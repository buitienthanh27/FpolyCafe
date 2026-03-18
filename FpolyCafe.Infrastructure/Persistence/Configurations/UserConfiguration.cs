using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpolyCafe.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.HasIndex(u => u.Username).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.FullName).HasMaxLength(100);
        builder.Property(u => u.Role).IsRequired();
    }
}
