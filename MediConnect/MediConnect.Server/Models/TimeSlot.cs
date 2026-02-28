using System;
using System.Collections.Generic;

namespace MediConnect.Server.Models;

public partial class TimeSlot
{
    public int SlotId { get; set; }

    public int ScheduleId { get; set; }

    public int UserId { get; set; }

    public DateOnly SlotDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int MaxCapacity { get; set; }

    public int BookedCount { get; set; }

    public bool IsAvailable { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual DoctorSchedule Schedule { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
