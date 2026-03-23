using System;

namespace FpolyCafe.Domain.Entities;

public class AttendanceAdjustment
{
    public int AdjustmentId { get; set; }
    public int AttendanceId { get; set; }
    public int AdjustedByUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? OldCheckInTime { get; set; }
    public DateTime? NewCheckInTime { get; set; }
    public DateTime? OldCheckOutTime { get; set; }
    public DateTime? NewCheckOutTime { get; set; }
    public int OldWorkedMinutes { get; set; }
    public int NewWorkedMinutes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Attendance Attendance { get; set; } = null!;
    public virtual User AdjustedByUser { get; set; } = null!;
}
