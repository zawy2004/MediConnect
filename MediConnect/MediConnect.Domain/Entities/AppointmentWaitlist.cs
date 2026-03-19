using System;
using System.Collections.Generic;

namespace MediConnect.Domain.Entities;

public partial class AppointmentWaitlist
{
    public int WaitlistId { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public int? SpecialtyId { get; set; }

    public DateOnly? PreferredDate { get; set; }

    public TimeOnly? PreferredTime { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? NotifiedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User Doctor { get; set; } = null!;

    public virtual User Patient { get; set; } = null!;

    public virtual Specialty? Specialty { get; set; }
}
