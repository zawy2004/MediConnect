using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class NotificationsModel : PageModel
{
    private readonly IPatientPortalService _patientPortalService;

    public NotificationsModel(IPatientPortalService patientPortalService)
    {
        _patientPortalService = patientPortalService;
    }

    public List<PatientNotificationItemDto> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        var patientId = GetUserId();
        // Đồng bộ với icon header: đánh dấu đã đọc khi mở trang (cùng nguồn IN_APP / IsRead).
        await _patientPortalService.MarkAllInAppNotificationsReadAsync(patientId);
        Items = await _patientPortalService.GetRecentInAppNotificationsAsync(patientId, 30);
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

