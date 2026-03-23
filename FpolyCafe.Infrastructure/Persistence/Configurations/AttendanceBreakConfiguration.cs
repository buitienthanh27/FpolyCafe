using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FpolyCafe.Infrastructure.Persistence.Configurations;

public class AttendanceBreakConfiguration : IEntityTypeConfiguration<AttendanceBreak>
{
    public void Configure(EntityTypeBuilder<AttendanceBreak> builder)
    {
        builder.HasKey(x => x.BreakId);
        builder.Property(x => x.Note).HasMaxLength(250);

        builder.HasOne(x => x.Attendance)
            .WithMany(x => x.Breaks)
            .HasForeignKey(x => x.AttendanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
