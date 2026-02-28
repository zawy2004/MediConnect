using System;
using System.Collections.Generic;

namespace MediConnect.Server.Models;

public partial class Complaint
{
    public int ComplaintId { get; set; }

    public int PatientId { get; set; }

    public int? DoctorId { get; set; }

    public int? AppointmentId { get; set; }

    public string Subject { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Status { get; set; } = null!;

    public int? ResolvedBy { get; set; }

    public string? ResolutionNote { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual User? Doctor { get; set; }

    public virtual User Patient { get; set; } = null!;

    public virtual User? ResolvedByNavigation { get; set; }
}
