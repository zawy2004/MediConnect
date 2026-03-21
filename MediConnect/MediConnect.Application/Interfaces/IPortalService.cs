using MediConnect.Application.DTOs;

namespace MediConnect.Application.Interfaces;

public interface IPortalService
{
    Task<PatientDashboardDto> GetPatientDashboardAsync(int patientId);
    Task<DoctorDashboardDto> GetDoctorDashboardAsync(int doctorUserId);
    Task<AdminDashboardDto> GetAdminDashboardAsync();
    Task<bool> UpdateDoctorProfileAsync(int doctorUserId, string bio, decimal consultationFee, string? insuranceAccepted, string? location);
}
