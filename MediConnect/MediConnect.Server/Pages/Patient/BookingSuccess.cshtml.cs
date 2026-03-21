using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class BookingSuccessModel : PageModel
{
    private readonly IPatientPortalService _patientPortalService;

    public BookingSuccessModel(IPatientPortalService patientPortalService)
    {
        _patientPortalService = patientPortalService;
    }

    [BindProperty(SupportsGet = true)]
    public int AppointmentId { get; set; }

    public PatientBookingSuccessDto? Data { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (AppointmentId <= 0)
        {
            return RedirectToPage("/Patient/Appointments");
        }

        Data = await _patientPortalService.GetBookingSuccessAsync(AppointmentId);
        if (Data == null)
        {
            return NotFound();
        }

        return Page();
    }
}
