using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class SymptomAssessmentModel : PageModel
{
    private readonly IPatientPortalService _patientPortalService;

    public SymptomAssessmentModel(IPatientPortalService patientPortalService)
    {
        _patientPortalService = patientPortalService;
    }

    [BindProperty]
    public string SymptomText { get; set; } = string.Empty;

    public PatientTriageResultDto Result { get; set; } = new()
    {
        AssistantReply = "Mô tả triệu chứng để MediConnect AI gợi ý bác sĩ phù hợp.",
        SuggestedSpecialty = "Tổng quát",
        RiskScore = 20
    };

    public async Task OnGetAsync()
    {
        Result = await _patientPortalService.AnalyzeSymptomsAsync(GetUserId(), "mệt nhẹ");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(SymptomText))
        {
            ModelState.AddModelError(nameof(SymptomText), "Vui lòng nhập triệu chứng.");
            return Page();
        }

        Result = await _patientPortalService.AnalyzeSymptomsAsync(GetUserId(), SymptomText);
        return Page();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
