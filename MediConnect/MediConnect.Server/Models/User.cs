using System;
using System.Collections.Generic;

namespace MediConnect.Server.Models;

public partial class User
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Gender { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<AiRecommendation> AiRecommendations { get; set; } = new List<AiRecommendation>();

    public virtual ICollection<Appointment> AppointmentDoctors { get; set; } = new List<Appointment>();

    public virtual ICollection<Appointment> AppointmentPatients { get; set; } = new List<Appointment>();

    public virtual ICollection<AppointmentWaitlist> AppointmentWaitlistDoctors { get; set; } = new List<AppointmentWaitlist>();

    public virtual ICollection<AppointmentWaitlist> AppointmentWaitlistPatients { get; set; } = new List<AppointmentWaitlist>();

    public virtual ICollection<Complaint> ComplaintDoctors { get; set; } = new List<Complaint>();

    public virtual ICollection<Complaint> ComplaintPatients { get; set; } = new List<Complaint>();

    public virtual ICollection<Complaint> ComplaintResolvedByNavigations { get; set; } = new List<Complaint>();

    public virtual ICollection<DoctorProfile> DoctorProfileApprovedByNavigations { get; set; } = new List<DoctorProfile>();

    public virtual DoctorProfile? DoctorProfileUser { get; set; }

    public virtual ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new List<DoctorSchedule>();

    public virtual ICollection<DoctorSpecialty> DoctorSpecialties { get; set; } = new List<DoctorSpecialty>();

    public virtual ICollection<ExternalCalendarIntegration> ExternalCalendarIntegrations { get; set; } = new List<ExternalCalendarIntegration>();

    public virtual ICollection<MedicalRecord> MedicalRecordDoctors { get; set; } = new List<MedicalRecord>();

    public virtual ICollection<MedicalRecord> MedicalRecordPatients { get; set; } = new List<MedicalRecord>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<PromotionalCampaign> PromotionalCampaigns { get; set; } = new List<PromotionalCampaign>();

    public virtual ICollection<Review> ReviewDoctors { get; set; } = new List<Review>();

    public virtual ICollection<Review> ReviewPatients { get; set; } = new List<Review>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();

    public virtual ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
}
