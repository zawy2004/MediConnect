using MediConnect.Domain.Constants;

namespace MediConnect.Application.DTOs;

public class PatientPortalDashboardDto
{
    public string PatientName { get; set; } = "Bệnh nhân";
    public string HealthAlertTitle { get; set; } = "AI Health Alert";
    public string HealthAlertMessage { get; set; } = "Bạn có thể kiểm tra triệu chứng để nhận tư vấn chuyên khoa phù hợp.";
    public int UpcomingAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public List<AppointmentListDto> Schedule { get; set; } = new();
    public List<string> Insights { get; set; } = new();
}

public class PatientDoctorScheduleDto
{
    public int DoctorUserId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public List<PatientDoctorSlotDto> Slots { get; set; } = new();
}

public class PatientDoctorSlotDto
{
    public int SlotId { get; set; }
    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public string StatusLabel { get; set; } = "Trống";
}

public class PatientDoctorProfileDto
{
    public DoctorDetailDto? Doctor { get; set; }
    public List<PatientDoctorSlotDto> NextSlots { get; set; } = new();
    public int TotalVisits { get; set; }
    public decimal SatisfiedPercent { get; set; }
    public int AwardsCount { get; set; }
}

public class PatientAppointmentManagerDto
{
    public int TotalAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public List<AppointmentListDto> Appointments { get; set; } = new();
}

public class PatientTriageResultDto
{
    public string SymptomText { get; set; } = string.Empty;
    public string AssistantReply { get; set; } = string.Empty;
    public string SuggestedSpecialty { get; set; } = "Tổng quát";
    public int RiskScore { get; set; }
    public List<DoctorListDto> SuggestedDoctors { get; set; } = new();
}

public class PatientPaymentConfirmDto
{
    public int AppointmentId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? SpecialtyName { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public string? Location { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public List<string> PaymentMethods { get; set; } = new() { "VNPAY", "MOMO", "BANK_TRANSFER", "CASH" };
}

public class PatientPaymentResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public int PaymentId { get; set; }
}

public class PatientBookingSuccessDto
{
    public int AppointmentId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string? SpecialtyName { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public string? Location { get; set; }
    public string Status { get; set; } = AppointmentStatus.Pending;
    public string? TransactionId { get; set; }
}

public class PatientProfilePortalDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string BloodType { get; set; } = "O+";
    public decimal Bmi { get; set; } = 22.4m;
    public List<PatientMetricPointDto> WeightTrend { get; set; } = new();
    public List<PatientMetricPointDto> BloodPressureTrend { get; set; } = new();
    public List<string> Alerts { get; set; } = new();
}

public class PatientMetricPointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}
