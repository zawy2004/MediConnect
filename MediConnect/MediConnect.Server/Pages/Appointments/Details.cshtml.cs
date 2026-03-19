using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Appointments;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IAppointmentService _appointmentService;

    public DetailsModel(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public AppointmentDetailDto? Appointment { get; set; }

    public string GetBadgeClass(string status) => StatusHelper.GetBadgeClass(status);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Appointment = await _appointmentService.GetAppointmentDetailAsync(id);
        if (Appointment == null) return NotFound();
        return Page();
    }
}
