using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class WaitlistModel : PageModel
{
    private readonly IDoctorPortalService _doctorPortalService;

    public WaitlistModel(IDoctorPortalService doctorPortalService)
    {
        _doctorPortalService = doctorPortalService;
    }

    public DoctorWaitlistDto Data { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Data = await _doctorPortalService.GetWaitlistAsync(GetUserId());
    }

    public async Task<IActionResult> OnPostNotifyNextAsync()
    {
        var ok = await _doctorPortalService.NotifyNextWaitlistAsync(GetUserId());
        StatusMessage = ok ? "Đã gửi thông báo cho bệnh nhân kế tiếp." : "Không có bệnh nhân chờ để thông báo.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveUpAsync(int waitlistId)
    {
        await _doctorPortalService.MoveWaitlistUpAsync(waitlistId, GetUserId());
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveDownAsync(int waitlistId)
    {
        await _doctorPortalService.MoveWaitlistDownAsync(waitlistId, GetUserId());
        return RedirectToPage();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
