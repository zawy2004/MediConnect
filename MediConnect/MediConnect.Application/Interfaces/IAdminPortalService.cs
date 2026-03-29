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
    Task<AdminComplaintDto> GetComplaintsAsync(string? category, string? priority, string? status, int? selectedComplaintId);
    Task<ComplaintDetailDto?> GetComplaintDetailAsync(int complaintId);
    Task<bool> UpdateComplaintAsync(UpdateComplaintDto dto, int adminUserId);
    Task<bool> EscalateComplaintAsync(int complaintId, int adminUserId, string escalationReason);
    Task<bool> SetFollowUpReminderAsync(int complaintId, DateTime reminderDate);
    Task<bool> AutoAssignComplaintAsync(int complaintId, string category, string priority);
    Task<bool> ResolveComplaintAsync(int complaintId, int adminUserId, string resolutionNote, string nextStatus);
    Task<AdminMailNotificationDto> GetMailNotificationAsync();
    Task<int> SendMailNotificationAsync(string targetRole, int? targetUserId, string title, string body, string notificationType);
    
    // User Management - New Methods
    Task<UserDetailDto?> GetUserDetailAsync(int userId);
    Task<bool> ToggleUserStatusAsync(int userId, bool isActive, int adminUserId);
    Task<bool> UpdateUserAsync(EditUserDto dto, int adminUserId);
    Task<bool> ResetUserPasswordAsync(int userId, int adminUserId);
    Task<List<UserActivityItemDto>> GetUserActivityAsync(int userId, int limit = 50);
    Task<byte[]> ExportUsersExcelAsync(List<int>? userIds = null);
}
