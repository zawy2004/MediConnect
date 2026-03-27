using MediConnect.Application.DTOs;

namespace MediConnect.Application.Interfaces;

public interface IAdminPortalService
{
    Task<AdminOverviewDto> GetOverviewAsync();
    Task<AdminUserManagementDto> GetUserManagementAsync(string? searchTerm, string? roleName);
    Task<bool> ApproveDoctorAsync(int doctorProfileId, int adminUserId);
    Task<bool> RejectDoctorAsync(int doctorProfileId, int adminUserId);
    Task<AdminSpecialtyDepartmentDto> GetSpecialtyDepartmentAsync();
    Task<AdminSpecialtyConfigDto?> GetSpecialtyConfigAsync(int specialtyId);
    Task<bool> SaveSpecialtyConfigAsync(SaveSpecialtyConfigDto dto);
    Task<AdminStatisticsDto> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate);
    Task<AdminMonitoringDto> GetMonitoringAsync();
    Task<AdminComplaintDto> GetComplaintsAsync(int? selectedComplaintId);
    Task<bool> ResolveComplaintAsync(int complaintId, int adminUserId, string resolutionNote, string nextStatus);
    Task<AdminMailNotificationDto> GetMailNotificationAsync();
    Task<int> SendMailNotificationAsync(string targetRole, int? targetUserId, string title, string body, string notificationType);
}
