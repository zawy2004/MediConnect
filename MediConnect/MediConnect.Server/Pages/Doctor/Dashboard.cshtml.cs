using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using MediConnect.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class DashboardModel : PageModel
{
    private readonly IDoctorPortalService _doctorPortalService;

    public DashboardModel(IDoctorPortalService doctorPortalService)
    {
        _doctorPortalService = doctorPortalService;
    }

    public DoctorOverviewDto Data { get; set; } = new();

    public string GetBadgeClass(string status) => StatusHelper.GetBadgeClass(status);

    public async Task OnGetAsync()
    {
        Data = await _doctorPortalService.GetOverviewAsync(GetUserId());
    }

    public async Task<IActionResult> OnPostConfirmAsync(int appointmentId)
    {
        await _doctorPortalService.ConfirmRequestAsync(appointmentId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int appointmentId, string? reason)
    {
        await _doctorPortalService.RejectRequestAsync(appointmentId, reason ?? string.Empty);
        return RedirectToPage();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
