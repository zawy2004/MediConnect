using System;
using System.Collections.Generic;

namespace MediConnect.Server.Models;

public partial class DoctorSpecialty
{
    public int DoctorSpecialtyId { get; set; }

    public int UserId { get; set; }

    public int SpecialtyId { get; set; }

    public bool IsPrimary { get; set; }

    public virtual Specialty Specialty { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
