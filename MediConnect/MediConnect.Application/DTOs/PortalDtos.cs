namespace MediConnect.Application.DTOs;

public class PatientDashboardDto
{
    public string PatientName { get; set; } = "Bệnh nhân";
    public int TotalAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public List<AppointmentListDto> UpcomingList { get; set; } = new();
    public List<DoctorListDto> RecommendedDoctors { get; set; } = new();
}

public class DoctorDashboardDto
{
    public string DoctorName { get; set; } = "Bác sĩ";
    public string? DepartmentName { get; set; }
    public int TotalAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int ConfirmedAppointments { get; set; }
    public List<AppointmentListDto> UpcomingList { get; set; } = new();
    public DoctorDetailDto? Profile { get; set; }
}

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalPatients { get; set; }
    public int TotalAppointments { get; set; }
    public int PendingDoctorApprovals { get; set; }
    public int PendingAppointments { get; set; }
    public List<DoctorApprovalItemDto> PendingDoctors { get; set; } = new();
    public List<SystemUserItemDto> RecentUsers { get; set; } = new();
}

public class DoctorApprovalItemDto
{
    public int DoctorProfileId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? Email { get; set; }
    public int YearsOfExperience { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SystemUserItemDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
