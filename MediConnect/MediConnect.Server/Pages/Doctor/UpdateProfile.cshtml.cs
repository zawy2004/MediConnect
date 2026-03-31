using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class UpdateProfileModel : PageModel
{
    private readonly IDoctorPortalService _doctorPortalService;

    public UpdateProfileModel(IDoctorPortalService doctorPortalService)
    {
        _doctorPortalService = doctorPortalService;
    }

    [BindProperty]
    [Required]
    public string Bio { get; set; } = string.Empty;

    [BindProperty]
    [Range(0, 100000000)]
    public decimal ConsultationFee { get; set; }

    [BindProperty]
    public string? InsuranceAccepted { get; set; }

    [BindProperty]
    public string? Location { get; set; }

    public DoctorDetailDto? DoctorProfile { get; set; }
    public string MembershipPlan { get; set; } = "PREMIUM";
    public DateOnly MembershipExpiresAt { get; set; }
    public string AiTrend { get; set; } = string.Empty;
    public string AiRecommendation { get; set; } = string.Empty;
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadProfile();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadProfile();
            return Page();
        }

        var updated = await _doctorPortalService.UpdateSpecialtyProfileAsync(GetUserId(), Bio, ConsultationFee, InsuranceAccepted, Location);
        if (updated)
        {
            SuccessMessage = "Cập nhật hồ sơ thành công.";
        }

        await LoadProfile();
        return Page();
    }

    private async Task LoadProfile()
    {
        var data = await _doctorPortalService.GetSpecialtyProfileAsync(GetUserId());
        DoctorProfile = data?.Doctor;
        MembershipPlan = data?.MembershipPlan ?? "PREMIUM";
        MembershipExpiresAt = data?.MembershipExpiresAt ?? DateOnly.FromDateTime(DateTime.Today.AddMonths(1));
        AiTrend = data?.AiTrend ?? string.Empty;
        AiRecommendation = data?.AiRecommendation ?? string.Empty;

        if (DoctorProfile != null)
        {
            Bio = DoctorProfile.Bio ?? string.Empty;
            ConsultationFee = DoctorProfile.ConsultationFee;
            InsuranceAccepted = DoctorProfile.InsuranceAccepted;
            Location = DoctorProfile.Location;
        }
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
