using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using MediConnect.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class AppointmentsModel : PageModel
{
    private readonly IPatientPortalService _patientPortalService;
    private readonly IAppointmentService _appointmentService;

    public AppointmentsModel(IPatientPortalService patientPortalService, IAppointmentService appointmentService)
    {
        _patientPortalService = patientPortalService;
        _appointmentService = appointmentService;
    }

    public PatientAppointmentManagerDto Data { get; set; } = new();

    public string GetBadgeClass(string status) => StatusHelper.GetBadgeClass(status);

    public async Task OnGetAsync()
    {
        Data = await _patientPortalService.GetAppointmentManagerAsync(GetUserId());
    }

    public async Task<IActionResult> OnPostCancelAsync(int appointmentId)
    {
        await _appointmentService.CancelAppointmentAsync(new CancelAppointmentDto
        {
            AppointmentId = appointmentId,
            CancelReason = "Bệnh nhân hủy từ Appointment Manager"
        });

        return RedirectToPage();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
