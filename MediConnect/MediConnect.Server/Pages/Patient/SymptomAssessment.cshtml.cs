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
        AssistantReply = "Chào bạn, mình là trợ lý sức khỏe MediConnect. Bạn cứ mô tả tự nhiên triệu chứng đang gặp phải, thời gian kéo dài và mức độ khó chịu nhé.",
        SuggestedSpecialty = "Tổng quát",
        RiskScore = 20
    };

    public Task OnGetAsync()
    {
        // Keep first load lightweight to avoid UI freezing and let the user start the chat naturally.
        return Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(SymptomText))
        {
            ModelState.AddModelError(nameof(SymptomText), "Vui lòng nhập triệu chứng.");
            return Page();
        }

        Result = await AnalyzeAndEnhanceAsync(SymptomText);
        return Page();
    }

    public async Task<IActionResult> OnPostChatAsync()
    {
        if (string.IsNullOrWhiteSpace(SymptomText))
        {
            return new JsonResult(new
            {
                success = false,
                error = "Vui lòng nhập triệu chứng."
            });
        }

        var result = await AnalyzeAndEnhanceAsync(SymptomText);
        return new JsonResult(new
        {
            success = true,
            symptomText = result.SymptomText,
            assistantReply = result.AssistantReply,
            suggestedSpecialty = result.SuggestedSpecialty,
            riskScore = result.RiskScore,
            suggestedDoctors = result.SuggestedDoctors.Select(d => new
            {
                doctorProfileId = d.DoctorProfileId,
                userId = d.UserId,
                fullName = d.FullName,
                departmentName = d.DepartmentName,
                averageRating = d.AverageRating,
                yearsOfExperience = d.YearsOfExperience
            })
        });
    }

    private async Task<PatientTriageResultDto> AnalyzeAndEnhanceAsync(string symptomText)
    {
        var result = await _patientPortalService.AnalyzeSymptomsAsync(GetUserId(), symptomText);
        result.AssistantReply = MakeReplyMoreNatural(result.AssistantReply, result.SuggestedSpecialty);
        return result;
    }

    private static string MakeReplyMoreNatural(string reply, string specialty)
    {
        if (string.IsNullOrWhiteSpace(reply))
        {
            return $"Mình đã ghi nhận triệu chứng của bạn. Trước mắt bạn có thể ưu tiên khám {specialty} để được bác sĩ đánh giá trực tiếp.";
        }

        var normalized = reply.Trim();
        if (!normalized.EndsWith(".") && !normalized.EndsWith("!") && !normalized.EndsWith("?"))
        {
            normalized += ".";
        }

        return $"{normalized} Nếu có thêm dấu hiệu mới, bạn cứ nhắn tiếp để mình hỗ trợ sát hơn.";
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
