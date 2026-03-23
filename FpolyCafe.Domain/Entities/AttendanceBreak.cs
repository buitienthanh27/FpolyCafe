using System;
using FpolyCafe.Domain.Enums;

namespace FpolyCafe.Domain.Entities;

public class AttendanceBreak
{
    public int BreakId { get; set; }
    public int AttendanceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public BreakStatus Status { get; set; } = BreakStatus.Active;
    public string? Note { get; set; }

    public virtual Attendance Attendance { get; set; } = null!;
}
