using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Admin;

[Authorize(Roles = RoleNames.Admin)]
public class MailNotificationModel : PageModel
{
    private readonly IAdminPortalService _adminPortalService;

    public MailNotificationModel(IAdminPortalService adminPortalService)
    {
        _adminPortalService = adminPortalService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty]
    public string TargetRole { get; set; } = RoleNames.Patient;

    [BindProperty]
    public int? TargetUserId { get; set; }

    [BindProperty]
    public string NotificationType { get; set; } = "SYSTEM";

    [BindProperty]
    public string NotificationTitle { get; set; } = string.Empty;

    [BindProperty]
    public string NotificationBody { get; set; } = string.Empty;

    public List<SystemUserItemDto> RecipientCandidates { get; set; } = new();

    public AdminMailNotificationDto Data { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostSendAsync()
    {
        if (string.IsNullOrWhiteSpace(NotificationTitle) || string.IsNullOrWhiteSpace(NotificationBody))
        {
            StatusMessage = "Vui lòng nhập đầy đủ tiêu đề và nội dung thông báo.";
            return RedirectToPage(new { SearchTerm, StatusFilter });
        }

        var sentCount = await _adminPortalService.SendMailNotificationAsync(
            TargetRole,
            TargetUserId,
            NotificationTitle,
            NotificationBody,
            NotificationType);

        StatusMessage = sentCount > 0
            ? $"Đã gửi {sentCount} thông báo qua Email."
            : "Không có người nhận phù hợp để gửi thông báo.";

        return RedirectToPage(new { SearchTerm, StatusFilter });
    }

    private async Task LoadDataAsync()
    {
        Data = await _adminPortalService.GetMailNotificationAsync();
        Data.Messages = ApplyMessageFilters(Data.Messages);

        var users = await _adminPortalService.GetUserManagementAsync(null, null);
        RecipientCandidates = users.Users
            .Where(u => u.IsActive && (u.RoleName == RoleNames.Patient || u.RoleName == RoleNames.Doctor))
            .OrderBy(u => u.FullName)
            .ToList();
    }

    private List<MailMessageItemDto> ApplyMessageFilters(IEnumerable<MailMessageItemDto> source)
    {
        var query = source;

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var term = SearchTerm.Trim();
            query = query.Where(m =>
                m.UserName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || m.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
                || m.Body.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(StatusFilter) && !string.Equals(StatusFilter, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(m => string.Equals(m.Status, StatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .OrderByDescending(m => m.CreatedAt)
            .ToList();
    }
}