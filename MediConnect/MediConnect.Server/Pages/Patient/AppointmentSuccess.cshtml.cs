using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class AppointmentSuccessModel : PageModel
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentSuccessModel(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public AppointmentDetailDto? Appointment { get; set; }

    public async Task<IActionResult> OnGetAsync(int appointmentId)
    {
        Appointment = await _appointmentService.GetAppointmentDetailAsync(appointmentId);
        if (Appointment == null)
        {
            return NotFound();
        }

        return Page();
    }
}
