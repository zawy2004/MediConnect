using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class SearchDoctorsModel : PageModel
{
    private readonly IDoctorService _doctorService;

    public SearchDoctorsModel(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SpecialtyId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DepartmentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MinRating { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Insurance { get; set; }

    public List<DoctorListDto> Doctors { get; set; } = new();
    public SelectList SpecialtyList { get; set; } = default!;
    public SelectList DepartmentList { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync()
    {
        var specialties = await _doctorService.GetActiveSpecialtiesAsync();
        var departments = await _doctorService.GetActiveDepartmentsAsync();

        SpecialtyList = new SelectList(specialties, "SpecialtyId", "SpecialtyName");
        DepartmentList = new SelectList(departments, "DepartmentId", "DepartmentName");

        Doctors = await _doctorService.SearchDoctorsAsync(new DoctorSearchFilterDto
        {
            SearchTerm = SearchTerm,
            SpecialtyId = SpecialtyId,
            DepartmentId = DepartmentId
        });

        if (MinRating.HasValue)
        {
            Doctors = Doctors.Where(d => d.AverageRating >= MinRating.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(Insurance))
        {
            Doctors = Doctors.Where(d => (d.InsuranceAccepted ?? string.Empty).Contains(Insurance, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (Doctors.Count == 0)
        {
            return RedirectToPage("/Patient/NoDoctorsFound", new
            {
                searchTerm = SearchTerm,
                specialtyId = SpecialtyId,
                departmentId = DepartmentId,
                minRating = MinRating,
                insurance = Insurance
            });
        }

        return Page();
    }
}
