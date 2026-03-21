using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class DoctorScheduleModel : PageModel
{
    private readonly IPatientPortalService _patientPortalService;

    public DoctorScheduleModel(IPatientPortalService patientPortalService)
    {
        _patientPortalService = patientPortalService;
    }

    [BindProperty(SupportsGet = true)]
    public int DoctorUserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? FromDate { get; set; }

    public PatientDoctorScheduleDto? Data { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (DoctorUserId <= 0)
        {
            return RedirectToPage("/Patient/SearchDoctors");
        }

        var fromDate = FromDate ?? DateOnly.FromDateTime(DateTime.Today);
        Data = await _patientPortalService.GetDoctorScheduleAsync(DoctorUserId, fromDate, 7);
        if (Data == null)
        {
            return NotFound();
        }

        return Page();
    }
}
