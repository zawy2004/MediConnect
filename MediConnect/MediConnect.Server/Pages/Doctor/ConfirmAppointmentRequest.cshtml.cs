using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class ConfirmAppointmentRequestModel : PageModel
{
    private readonly IDoctorPortalService _doctorPortalService;

    public ConfirmAppointmentRequestModel(IDoctorPortalService doctorPortalService)
    {
        _doctorPortalService = doctorPortalService;
    }

    public List<DoctorAppointmentRequestDto> Requests { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Requests = await _doctorPortalService.GetPendingRequestsAsync(GetUserId());
    }

    public async Task<IActionResult> OnPostConfirmAsync(int appointmentId)
    {
        await _doctorPortalService.ConfirmRequestAsync(appointmentId);
        StatusMessage = "Đã xác nhận yêu cầu hẹn khám.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int appointmentId, string? reason)
    {
        await _doctorPortalService.RejectRequestAsync(appointmentId, reason ?? "Không phù hợp lịch trình.");
        StatusMessage = "Đã từ chối yêu cầu hẹn khám.";
        return RedirectToPage();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
