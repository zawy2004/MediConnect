using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class DoctorSearchModel : PageModel
{
    private readonly IDoctorService _doctorService;

    public DoctorSearchModel(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SpecialtyId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DepartmentId { get; set; }

    public List<DoctorListDto> Doctors { get; set; } = new();
    public SelectList SpecialtyList { get; set; } = default!;
    public SelectList DepartmentList { get; set; } = default!;

    public async Task OnGetAsync()
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
    }
}
