using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class ConsultationModel : PageModel
{
    private readonly IDoctorPortalService _doctorPortalService;

    public ConsultationModel(IDoctorPortalService doctorPortalService)
    {
        _doctorPortalService = doctorPortalService;
    }

    [BindProperty(SupportsGet = true)]
    public int AppointmentId { get; set; }

    [BindProperty]
    public string Symptoms { get; set; } = string.Empty;

    [BindProperty]
    public string Diagnosis { get; set; } = string.Empty;

    [BindProperty]
    public string TreatmentPlan { get; set; } = string.Empty;

    [BindProperty]
    public string Prescription { get; set; } = string.Empty;

    [BindProperty]
    public string Notes { get; set; } = string.Empty;

    public DoctorConsultationDto? Data { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (AppointmentId > 0)
        {
            Data = await _doctorPortalService.GetConsultationAsync(GetUserId(), AppointmentId);
            if (Data != null)
            {
                Symptoms = Data.Symptoms ?? string.Empty;
                Diagnosis = Data.Diagnosis ?? string.Empty;
                TreatmentPlan = Data.TreatmentPlan ?? string.Empty;
                Prescription = Data.Prescription ?? string.Empty;
                Notes = Data.Notes ?? string.Empty;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCompleteAsync()
    {
        if (AppointmentId <= 0)
        {
            return RedirectToPage("/Doctor/Dashboard");
        }

        var success = await _doctorPortalService.CompleteConsultationAsync(
            GetUserId(),
            AppointmentId,
            Symptoms,
            Diagnosis,
            TreatmentPlan,
            Prescription,
            Notes);

        if (!success)
        {
            StatusMessage = "Không thể hoàn tất khám. Vui lòng thử lại.";
            return RedirectToPage("/Doctor/Dashboard");
        }

        StatusMessage = "Hoàn tất khám và lưu bệnh án.";
        return RedirectToPage(new { AppointmentId });
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
