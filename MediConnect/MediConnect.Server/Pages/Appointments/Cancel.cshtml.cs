using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Appointments;

[Authorize]
public class CancelModel : PageModel
{
    private readonly IAppointmentService _appointmentService;

    public CancelModel(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [BindProperty]
    public int AppointmentId { get; set; }

    [BindProperty]
    public string? CancelReason { get; set; }

    public AppointmentDetailDto? Appointment { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Appointment = await _appointmentService.GetAppointmentDetailAsync(id);
        if (Appointment == null) return NotFound();

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (Appointment.PatientId != currentUserId) return Forbid();

        AppointmentId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var detail = await _appointmentService.GetAppointmentDetailAsync(AppointmentId);
        if (detail == null) return NotFound();

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (detail.PatientId != currentUserId) return Forbid();

        var result = await _appointmentService.CancelAppointmentAsync(new CancelAppointmentDto
        {
            AppointmentId = AppointmentId,
            CancelReason = CancelReason
        });

        if (!result.Success)
        {
            Appointment = detail;
            return Page();
        }

        return RedirectToPage("/Appointments/Index");
    }
}
