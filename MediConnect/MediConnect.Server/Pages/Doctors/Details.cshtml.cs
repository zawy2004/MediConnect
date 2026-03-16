using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Doctors;

public class DetailsModel : PageModel
{
    private readonly IDoctorService _doctorService;

    public DetailsModel(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    public DoctorDetailDto? Doctor { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Doctor = await _doctorService.GetDoctorDetailAsync(id);
        if (Doctor == null) return NotFound();
        return Page();
    }
}
