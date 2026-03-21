using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class SpecialtyProfileModel : PageModel
{
    private readonly IDoctorPortalService _doctorPortalService;

    public SpecialtyProfileModel(IDoctorPortalService doctorPortalService)
    {
        _doctorPortalService = doctorPortalService;
    }

    public DoctorSpecialtyProfileDto? Data { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Data = await _doctorPortalService.GetSpecialtyProfileAsync(GetUserId());
        if (Data == null) return NotFound();
        return Page();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
