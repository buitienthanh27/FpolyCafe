using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpolyCafe.Infrastructure.Persistence.Configurations;

public class MonthlyPayrollConfiguration : IEntityTypeConfiguration<MonthlyPayroll>
{
    public void Configure(EntityTypeBuilder<MonthlyPayroll> builder)
    {
        builder.HasKey(x => x.PayrollId);
        builder.Property(x => x.TotalNormalSalary).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalOvertimeSalary).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalSalary).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => new { x.EmployeeId, x.Month, x.Year }).IsUnique();

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.MonthlyPayrolls)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
