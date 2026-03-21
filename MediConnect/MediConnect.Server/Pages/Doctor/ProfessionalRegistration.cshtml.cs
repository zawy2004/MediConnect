using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class ProfessionalRegistrationModel : PageModel
{
    private readonly IDoctorPortalService _doctorPortalService;

    public ProfessionalRegistrationModel(IDoctorPortalService doctorPortalService)
    {
        _doctorPortalService = doctorPortalService;
    }

    [BindProperty]
    public string Bio { get; set; } = string.Empty;

    [BindProperty]
    public decimal ConsultationFee { get; set; }

    [BindProperty]
    public string? InsuranceAccepted { get; set; }

    [BindProperty]
    public string? Location { get; set; }

    public DoctorSpecialtyProfileDto? Data { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Data = await _doctorPortalService.GetSpecialtyProfileAsync(GetUserId());
        if (Data == null)
        {
            return NotFound();
        }

        Bio = Data.Doctor?.Bio ?? string.Empty;
        ConsultationFee = Data.Doctor?.ConsultationFee ?? 0;
        InsuranceAccepted = Data.Doctor?.InsuranceAccepted;
        Location = Data.Doctor?.Location;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _doctorPortalService.UpdateSpecialtyProfileAsync(GetUserId(), Bio, ConsultationFee, InsuranceAccepted, Location);
        StatusMessage = "Hồ sơ chuyên môn đã được cập nhật và gửi xét duyệt.";
        return RedirectToPage();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
