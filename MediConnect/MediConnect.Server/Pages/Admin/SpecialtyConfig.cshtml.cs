using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Admin;

[Authorize(Roles = RoleNames.Admin)]
public class SpecialtyConfigModel : PageModel
{
    private readonly IAdminPortalService _adminPortalService;

    public SpecialtyConfigModel(IAdminPortalService adminPortalService)
    {
        _adminPortalService = adminPortalService;
    }

    [BindProperty(SupportsGet = true)]
    public int SpecialtyId { get; set; }

    [BindProperty]
    public string SpecialtyName { get; set; } = string.Empty;

    [BindProperty]
    public string? Description { get; set; }

    [BindProperty]
    public string? IconUrl { get; set; }

    [BindProperty]
    public List<int> SelectedDoctorUserIds { get; set; } = new();

    public List<DoctorListDto> CandidateDoctors { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var data = await _adminPortalService.GetSpecialtyConfigAsync(SpecialtyId);
        if (data == null)
        {
            return NotFound();
        }

        SpecialtyName = data.SpecialtyName;
        Description = data.Description;
        IconUrl = data.IconUrl;
        SelectedDoctorUserIds = data.SelectedDoctorUserIds;
        CandidateDoctors = data.CandidateDoctors;

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var saved = await _adminPortalService.SaveSpecialtyConfigAsync(new SaveSpecialtyConfigDto
        {
            SpecialtyId = SpecialtyId,
            SpecialtyName = SpecialtyName,
            Description = Description,
            IconUrl = IconUrl,
            DoctorUserIds = SelectedDoctorUserIds
        });

        StatusMessage = saved ? "Đã lưu cấu hình chuyên khoa." : "Lưu thất bại.";
        return RedirectToPage(new { SpecialtyId });
    }
}
