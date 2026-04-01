namespace MediConnect.Application.DTOs;

public class DoctorOverviewDto
{
    public string DoctorName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public int TotalPatients { get; set; }
    public decimal SatisfactionPercent { get; set; }
    public int AppointmentsThisMonth { get; set; }
    public int AverageConsultationMinutes { get; set; }
    public List<DoctorScheduleItemDto> TodayTimeline { get; set; } = new();
    public List<DoctorScheduleDayDto> WeekSchedule { get; set; } = new();
    public List<DoctorScheduleMonthDayDto> MonthSchedule { get; set; } = new();
    public List<DoctorAppointmentRequestDto> PendingRequests { get; set; } = new();
}

public class DoctorScheduleItemDto
{
    public int AppointmentId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class DoctorScheduleDayDto
{
    public DateOnly Date { get; set; }
    public string Label { get; set; } = string.Empty;
    public List<DoctorScheduleItemDto> Appointments { get; set; } = new();
    public bool IsToday => Date == DateOnly.FromDateTime(DateTime.Today);
}

public class DoctorScheduleMonthDayDto
{
    public DateOnly Date { get; set; }
    public int AppointmentCount { get; set; }
    public bool HasUpcoming { get; set; }
    public bool HasCompleted { get; set; }
    public string Status => AppointmentCount == 0 ? "empty" : HasUpcoming ? "upcoming" : "completed";
}

public class DoctorAppointmentRequestDto
{
    public int AppointmentId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public string? Reason { get; set; }
    public string AiTriageSummary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
public class DoctorPatientListItemDto
{
    public int PatientId { get; set; }
    public int AppointmentId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PatientEmail { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public string? Reason { get; set; }
}

public class DoctorPatientGroupDto
{
    public string CategoryKey { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<DoctorPatientListItemDto> Patients { get; set; } = new();
}

public class DoctorCalendarEventDto
{
    public int AppointmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DoctorWaitlistItemDto
{
    public int WaitlistId { get; set; }
    public int Order { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateOnly? PreferredDate { get; set; }
    public TimeOnly? PreferredTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DoctorWaitlistDto
{
    public bool IsFullToday { get; set; }
    public int TotalCount { get; set; }
    public int AverageWaitMinutes { get; set; }
    public List<DoctorWaitlistItemDto> Items { get; set; } = new();
    public List<DoctorCalendarEventDto> CalendarEvents { get; set; } = new();
}

public class DoctorPatientRecordDto
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public int? Age { get; set; }
    public string? BloodType { get; set; }
    public string SymptomSummary { get; set; } = string.Empty;
    public string AllergyAlert { get; set; } = string.Empty;
    public int LastBloodPressureSystolic { get; set; }
    public int LastBloodPressureDiastolic { get; set; }
    public int LastHeartRate { get; set; }
    public List<PatientRecordHistoryDto> History { get; set; } = new();
    public List<PatientDocumentDto> Documents { get; set; } = new();
    public string ClinicalNoteDraft { get; set; } = string.Empty;
}

public class DoctorConsultationDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int? PatientAge { get; set; }
    public string? PatientGender { get; set; }
    public string? BloodType { get; set; }
    public string? AllergyAlert { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? Prescription { get; set; }
    public string? Notes { get; set; }
}

public class PatientRecordHistoryDto
{
    public int RecordId { get; set; }
    public DateOnly RecordDate { get; set; }
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    public string DoctorName { get; set; } = string.Empty;
}

public class PatientDocumentDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DoctorPerformanceDto
{
    public int TotalPatients { get; set; }
    public decimal ReturnRatePercent { get; set; }
    public decimal AverageRating { get; set; }
    public int AverageConsultationMinutes { get; set; }
    public List<DailyCountDto> WeeklyAppointments { get; set; } = new();
    public List<KeywordWeightDto> DiseaseDistribution { get; set; } = new();
    public List<DoctorFeedbackDto> RecentFeedbacks { get; set; } = new();
    public string AiInsight { get; set; } = string.Empty;
}

public class DoctorFeedbackDto
{
    public string PatientName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DoctorSpecialtyProfileDto
{
    public DoctorDetailDto? Doctor { get; set; }
    public string MembershipPlan { get; set; } = "PREMIUM";
    public DateOnly MembershipExpiresAt { get; set; }
    public string AiTrend { get; set; } = string.Empty;
    public string AiRecommendation { get; set; } = string.Empty;
}

public class DoctorMembershipPlanDto
{
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public bool IsRecommended { get; set; }
    public List<string> Features { get; set; } = new();
}

public class DoctorMembershipDto
{
    public string CurrentPlanCode { get; set; } = "PREMIUM";
    public List<DoctorMembershipPlanDto> Plans { get; set; } = new();
}

public class DoctorPaymentSummaryDto
{
    public string PlanCode { get; set; } = "PREMIUM";
    public string PlanName { get; set; } = "Gói Premium";
    public decimal Amount { get; set; }
    public int DurationDays { get; set; }
    public List<string> PaymentMethods { get; set; } = new();
}
