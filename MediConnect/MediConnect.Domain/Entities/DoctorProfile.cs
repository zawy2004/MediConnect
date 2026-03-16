using System;
using System.Collections.Generic;

namespace MediConnect.Domain.Entities;

public partial class DoctorProfile
{
    public int DoctorProfileId { get; set; }

    public int UserId { get; set; }

    public int? DepartmentId { get; set; }

    public string LicenseNumber { get; set; } = null!;

    public int YearsOfExperience { get; set; }

    public string? Education { get; set; }

    public string? Bio { get; set; }

    public decimal ConsultationFee { get; set; }

    public string? InsuranceAccepted { get; set; }

    public string? Location { get; set; }

    public string ApprovalStatus { get; set; } = null!;

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public decimal AverageRating { get; set; }

    public int TotalReviews { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual Department? Department { get; set; }

    public virtual User User { get; set; } = null!;
}
