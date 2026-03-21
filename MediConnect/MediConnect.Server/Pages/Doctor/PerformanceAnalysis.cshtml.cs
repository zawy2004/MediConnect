using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class PerformanceAnalysisModel : PageModel
{
    private readonly IDoctorPortalService _doctorPortalService;

    public PerformanceAnalysisModel(IDoctorPortalService doctorPortalService)
    {
        _doctorPortalService = doctorPortalService;
    }

    public DoctorPerformanceDto Data { get; set; } = new();

    public async Task OnGetAsync()
    {
        Data = await _doctorPortalService.GetPerformanceAsync(GetUserId());
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
