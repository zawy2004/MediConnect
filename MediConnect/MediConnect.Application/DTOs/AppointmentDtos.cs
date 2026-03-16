namespace MediConnect.Application.DTOs;

public class AppointmentListDto
{
    public int AppointmentId { get; set; }
    public string PatientName { get; set; } = null!;
    public string DoctorName { get; set; } = null!;
    public string? SpecialtyName { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class AppointmentDetailDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = null!;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = null!;
    public string? SpecialtyName { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = null!;
    public string? Notes { get; set; }
    public string? CancelReason { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAppointmentDto
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int SlotId { get; set; }
    public int? SpecialtyId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Reason { get; set; }
}

public class CancelAppointmentDto
{
    public int AppointmentId { get; set; }
    public string? CancelReason { get; set; }
}

public class AppointmentResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int? AppointmentId { get; set; }
}
