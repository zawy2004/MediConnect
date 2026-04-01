using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class PatientRecordModel : PageModel
{
    private readonly IDoctorPortalService _doctorPortalService;

    public PatientRecordModel(IDoctorPortalService doctorPortalService)
    {
        _doctorPortalService = doctorPortalService;
    }

    [BindProperty(SupportsGet = true)]
    public int PatientId { get; set; }

    public DoctorPatientRecordDto? Data { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (PatientId <= 0) return RedirectToPage("/Doctor/Patients");

        Data = await _doctorPortalService.GetPatientRecordAsync(GetUserId(), PatientId);
        if (Data == null) return RedirectToPage("/Doctor/Patients");

        return Page();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
