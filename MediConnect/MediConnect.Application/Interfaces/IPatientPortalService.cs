using MediConnect.Application.DTOs;

namespace MediConnect.Application.Interfaces;

public interface IPatientPortalService
{
    Task<PatientPortalDashboardDto> GetDashboardAsync(int patientId, int schedulePage = 1, int schedulePageSize = 4);
    Task<PatientDoctorScheduleDto?> GetDoctorScheduleAsync(int doctorUserId, DateOnly fromDate, int days);
    Task<PatientDoctorProfileDto?> GetDoctorProfileAsync(int doctorProfileId);
    Task<PatientAppointmentManagerDto> GetAppointmentManagerAsync(int patientId);
    Task<PatientTriageResultDto> AnalyzeSymptomsAsync(int patientId, string symptomText);
    Task<PatientPaymentConfirmDto?> GetPaymentConfirmAsync(int patientId, int appointmentId);
    Task<PatientPaymentResultDto> CompletePaymentAsync(int patientId, int appointmentId, string paymentMethod);
    Task<PatientBookingSuccessDto?> GetBookingSuccessAsync(int appointmentId);
    Task<PatientProfilePortalDto?> GetProfileAsync(int patientId);
    Task<bool> UpdateProfileAsync(int patientId, string fullName, string? phoneNumber, string? gender, DateOnly? dateOfBirth, string? address);

    Task<List<PatientNotificationItemDto>> GetRecentInAppNotificationsAsync(int patientId, int take = 20);
    Task<int> GetUnreadInAppNotificationCountAsync(int patientId);
    Task MarkAllInAppNotificationsReadAsync(int patientId);
    Task<List<PatientPaymentHistoryItemDto>> GetPaidAppointmentPaymentHistoryAsync(int patientId, int take = 20);
}
