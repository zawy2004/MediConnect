using System;
using System.Collections.Generic;

namespace MediConnect.Server.Models;

public partial class DoctorSchedule
{
    public int ScheduleId { get; set; }

    public int UserId { get; set; }

    public byte DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int SlotDurationMins { get; set; }

    public int MaxPatientsPerSlot { get; set; }

    public bool IsActive { get; set; }

    public DateOnly? EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();

    public virtual User User { get; set; } = null!;
}
