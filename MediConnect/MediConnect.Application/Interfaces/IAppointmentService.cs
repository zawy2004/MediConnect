using MediConnect.Application.DTOs;

namespace MediConnect.Application.Interfaces;

public interface IAppointmentService
{
    Task<List<AppointmentListDto>> GetAllAppointmentsAsync();
    Task<List<AppointmentListDto>> GetAppointmentsByPatientAsync(int patientId);
    Task<List<AppointmentListDto>> GetAppointmentsByDoctorAsync(int doctorId);
    Task<AppointmentDetailDto?> GetAppointmentDetailAsync(int appointmentId);
    Task<AppointmentResultDto> CreateAppointmentAsync(CreateAppointmentDto dto);
    Task<AppointmentResultDto> CancelAppointmentAsync(CancelAppointmentDto dto);
    Task<AppointmentResultDto> ConfirmAppointmentAsync(int appointmentId);
    Task<List<TimeSlotDto>> GetAvailableSlotsAsync();
    Task<List<TimeSlotDto>> GetAvailableSlotsByDoctorAsync(int doctorUserId, DateOnly fromDate, DateOnly toDate);
}
