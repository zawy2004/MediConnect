using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediConnect.Server.Pages.Doctors;

public class IndexModel : PageModel
{
    private readonly IDoctorService _doctorService;

    public IndexModel(IDoctorService doctorService)
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
        SpecialtyList = new SelectList(specialties, "SpecialtyId", "SpecialtyName");

        var departments = await _doctorService.GetActiveDepartmentsAsync();
        DepartmentList = new SelectList(departments, "DepartmentId", "DepartmentName");

        Doctors = await _doctorService.SearchDoctorsAsync(new DoctorSearchFilterDto
        {
            SearchTerm = SearchTerm,
            SpecialtyId = SpecialtyId,
            DepartmentId = DepartmentId
        });
    }
}
