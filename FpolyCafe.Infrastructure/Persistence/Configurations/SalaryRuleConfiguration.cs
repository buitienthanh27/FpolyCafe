using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpolyCafe.Infrastructure.Persistence.Configurations;

public class SalaryRuleConfiguration : IEntityTypeConfiguration<SalaryRule>
{
    public void Configure(EntityTypeBuilder<SalaryRule> builder)
    {
        builder.HasKey(x => x.SalaryRuleId);
        builder.Property(x => x.HourlyRate).HasColumnType("decimal(18,2)");
        builder.Property(x => x.OvertimeRate).HasColumnType("decimal(18,2)");
        builder.Property(x => x.NightShiftMultiplier).HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.SalaryRules)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
