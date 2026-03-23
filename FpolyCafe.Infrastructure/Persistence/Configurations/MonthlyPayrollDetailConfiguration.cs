using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpolyCafe.Infrastructure.Persistence.Configurations;

public class MonthlyPayrollDetailConfiguration : IEntityTypeConfiguration<MonthlyPayrollDetail>
{
    public void Configure(EntityTypeBuilder<MonthlyPayrollDetail> builder)
    {
        builder.HasKey(x => x.PayrollDetailId);
        builder.Property(x => x.SalaryAmount).HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Payroll)
            .WithMany(x => x.Details)
            .HasForeignKey(x => x.PayrollId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Attendance)
            .WithMany(x => x.PayrollDetails)
            .HasForeignKey(x => x.AttendanceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
