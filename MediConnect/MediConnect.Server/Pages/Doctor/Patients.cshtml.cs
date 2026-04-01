using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class PatientsModel : PageModel
{
    private readonly IDoctorPortalService _doctorPortalService;

    public PatientsModel(IDoctorPortalService doctorPortalService)
    {
        _doctorPortalService = doctorPortalService;
    }

    public List<DoctorPatientGroupDto> Groups { get; set; } = new();
    public DoctorPatientGroupDto? ActiveGroup { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Category { get; set; } = "confirmed";

    [BindProperty(SupportsGet = true)]
    public string SearchName { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string Specialty { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string Gender { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        Groups = await _doctorPortalService.GetPatientGroupsAsync(GetUserId());
        ActiveGroup = Groups.FirstOrDefault(g => g.CategoryKey == Category) ?? Groups.FirstOrDefault();

        if (ActiveGroup != null)
        {
            var filtered = ActiveGroup.Patients.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchName))
            {
                filtered = filtered.Where(p => p.PatientName.Contains(SearchName, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(Specialty))
            {
                filtered = filtered.Where(p => p.SpecialtyName.Contains(Specialty, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(Gender))
            {
                filtered = filtered.Where(p => p.Gender.Equals(Gender, StringComparison.OrdinalIgnoreCase));
            }

            ActiveGroup = new DoctorPatientGroupDto
            {
                CategoryKey = ActiveGroup.CategoryKey,
                Category = ActiveGroup.Category,
                Patients = filtered.ToList()
            };
        }
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
