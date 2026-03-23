using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpolyCafe.Infrastructure.Persistence.Configurations;

public class AttendanceAdjustmentConfiguration : IEntityTypeConfiguration<AttendanceAdjustment>
{
    public void Configure(EntityTypeBuilder<AttendanceAdjustment> builder)
    {
        builder.HasKey(x => x.AdjustmentId);
        builder.Property(x => x.Reason).IsRequired().HasMaxLength(500);

        builder.HasOne(x => x.Attendance)
            .WithMany(x => x.Adjustments)
            .HasForeignKey(x => x.AttendanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AdjustedByUser)
            .WithMany(x => x.AttendanceAdjustments)
            .HasForeignKey(x => x.AdjustedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
