using MediConnect.Application.DTOs;

namespace MediConnect.Application.Interfaces;

public interface IDoctorPortalService
{
    Task<DoctorOverviewDto> GetOverviewAsync(int doctorUserId);
    Task<List<DoctorAppointmentRequestDto>> GetPendingRequestsAsync(int doctorUserId);
    Task<bool> ConfirmRequestAsync(int appointmentId);
    Task<bool> RejectRequestAsync(int appointmentId, string reason);

    Task<DoctorWaitlistDto> GetWaitlistAsync(int doctorUserId);
    Task<bool> NotifyNextWaitlistAsync(int doctorUserId);
    Task<bool> MoveWaitlistUpAsync(int waitlistId, int doctorUserId);
    Task<bool> MoveWaitlistDownAsync(int waitlistId, int doctorUserId);

    Task<DoctorPatientRecordDto?> GetPatientRecordAsync(int doctorUserId, int patientId);
    Task<bool> SaveClinicalNoteAsync(int doctorUserId, int patientId, string note);

    Task<DoctorPerformanceDto> GetPerformanceAsync(int doctorUserId);

    Task<DoctorSpecialtyProfileDto?> GetSpecialtyProfileAsync(int doctorUserId);
    Task<bool> UpdateSpecialtyProfileAsync(int doctorUserId, string bio, decimal consultationFee, string? insuranceAccepted, string? location);

    Task<DoctorMembershipDto> GetMembershipAsync(int doctorUserId);
    Task<DoctorPaymentSummaryDto> GetPaymentSummaryAsync(int doctorUserId, string planCode);
    Task<bool> SubmitMembershipPaymentAsync(int doctorUserId, string planCode, string paymentMethod);
}
