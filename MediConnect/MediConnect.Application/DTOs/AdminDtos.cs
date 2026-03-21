namespace MediConnect.Application.DTOs;

public class AdminOverviewDto
{
    public int TotalUsers { get; set; }
    public int TotalAppointmentsToday { get; set; }
    public int ProcessingAppointmentsToday { get; set; }
    public decimal UptimePercent { get; set; }
    public decimal ErrorRatePercent { get; set; }
    public List<SpecialtyLoadDto> SpecialtyLoads { get; set; } = new();
    public List<DailyCountDto> TrendCounts { get; set; } = new();
}

public class AdminUserManagementDto
{
    public List<SystemUserItemDto> Users { get; set; } = new();
    public List<DoctorApprovalItemDto> PendingDoctors { get; set; } = new();
}

public class SpecialtyLoadDto
{
    public string SpecialtyName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DailyCountDto
{
    public DateOnly Date { get; set; }
    public int Count { get; set; }
}

public class AdminSpecialtyDepartmentDto
{
    public List<SpecialtyDepartmentItemDto> SpecialtyItems { get; set; } = new();
    public List<DepartmentDto> Departments { get; set; } = new();
}

public class SpecialtyDepartmentItemDto
{
    public int SpecialtyId { get; set; }
    public string SpecialtyName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int DoctorCount { get; set; }
}

public class AdminSpecialtyConfigDto
{
    public int SpecialtyId { get; set; }
    public string SpecialtyName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public List<DoctorListDto> CandidateDoctors { get; set; } = new();
    public List<int> SelectedDoctorUserIds { get; set; } = new();
}

public class SaveSpecialtyConfigDto
{
    public int SpecialtyId { get; set; }
    public string SpecialtyName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public List<int> DoctorUserIds { get; set; } = new();
}

public class AdminStatisticsDto
{
    public int TotalVisits { get; set; }
    public decimal Revenue { get; set; }
    public decimal CompletionRatePercent { get; set; }
    public decimal Csat { get; set; }
    public List<DailyCountDto> AppointmentTrend { get; set; } = new();
    public List<KeywordWeightDto> FeedbackKeywords { get; set; } = new();
}

public class KeywordWeightDto
{
    public string Keyword { get; set; } = string.Empty;
    public int Weight { get; set; }
}

public class AdminMonitoringDto
{
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
    public List<SystemLogItemDto> Logs { get; set; } = new();
    public int BackupProgressPercent { get; set; }
    public string PredictedLoadMessage { get; set; } = string.Empty;
}

public class SystemLogItemDto
{
    public DateTime CreatedAt { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Username { get; set; }
}

public class AdminComplaintDto
{
    public int TotalComplaints { get; set; }
    public int OpenComplaints { get; set; }
    public int ProcessingComplaints { get; set; }
    public List<ComplaintItemDto> Complaints { get; set; } = new();
    public ComplaintItemDto? SelectedComplaint { get; set; }
}

public class ComplaintItemDto
{
    public int ComplaintId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string? DoctorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ResolutionNote { get; set; }
}

public class AdminZaloNotificationDto
{
    public int TotalMessages { get; set; }
    public decimal SuccessRatePercent { get; set; }
    public decimal OpenRatePercent { get; set; }
    public decimal NoShowReductionPercent { get; set; }
    public List<ZaloMessageItemDto> Messages { get; set; } = new();
}

public class ZaloMessageItemDto
{
    public int NotificationId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
