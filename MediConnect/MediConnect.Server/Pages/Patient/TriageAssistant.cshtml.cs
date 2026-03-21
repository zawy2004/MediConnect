using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class TriageAssistantModel : PageModel
{
    private readonly IDoctorService _doctorService;

    public TriageAssistantModel(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [BindProperty]
    public string SymptomText { get; set; } = string.Empty;

    public string AssistantReply { get; set; } = "Mô tả triệu chứng để MediConnect AI gợi ý chuyên khoa và mức độ ưu tiên.";
    public string SuggestedSpecialty { get; set; } = "Tổng quát";
    public int RiskScore { get; set; } = 20;
    public List<DoctorListDto> SuggestedDoctors { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadDoctors();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(SymptomText))
        {
            ModelState.AddModelError(nameof(SymptomText), "Vui lòng nhập triệu chứng.");
            await LoadDoctors();
            return Page();
        }

        var text = SymptomText.ToLowerInvariant();

        if (text.Contains("tức ngực") || text.Contains("khó thở") || text.Contains("đau tim"))
        {
            SuggestedSpecialty = "Nội hô hấp";
            RiskScore = 80;
            AssistantReply = "Triệu chứng có dấu hiệu nguy cơ cao. Bạn nên ưu tiên khám chuyên khoa Nội hô hấp trong hôm nay.";
        }
        else if (text.Contains("ho") || text.Contains("sốt") || text.Contains("viêm"))
        {
            SuggestedSpecialty = "Nội tổng quát";
            RiskScore = 55;
            AssistantReply = "Mức độ trung bình, nên đặt lịch khám trong 24 giờ và theo dõi nhiệt độ, nhịp thở.";
        }
        else
        {
            SuggestedSpecialty = "Tư vấn tổng quát";
            RiskScore = 30;
            AssistantReply = "Mức độ thấp, bạn có thể đặt lịch tư vấn trước để được bác sĩ đánh giá chi tiết hơn.";
        }

        await LoadDoctors(SuggestedSpecialty.Contains("hô hấp") ? "hô hấp" : null);
        return Page();
    }

    private async Task LoadDoctors(string? searchTerm = null)
    {
        SuggestedDoctors = (await _doctorService.SearchDoctorsAsync(new DoctorSearchFilterDto
        {
            SearchTerm = searchTerm
        })).Take(3).ToList();
    }
}
