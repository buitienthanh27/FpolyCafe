using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpolyCafe.Infrastructure.Persistence.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.HasKey(x => x.AttendanceId);
        builder.Property(x => x.SalaryAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CheckInSource).HasMaxLength(50);
        builder.Property(x => x.CheckOutSource).HasMaxLength(50);
        builder.Property(x => x.CheckInIp).HasMaxLength(50);
        builder.Property(x => x.CheckOutIp).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasOne(x => x.Employee)
            .WithMany(x => x.Attendances)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
