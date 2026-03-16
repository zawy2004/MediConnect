using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Server.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Appointments;

[Authorize]
public class AppointmentListModel : PageModel
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentListModel(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    public List<AppointmentListDto> Appointments { get; set; } = new();

    public string GetBadgeClass(string status) => StatusHelper.GetBadgeClass(status);

    public async Task OnGetAsync()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role);

        Appointments = role == "DOCTOR"
            ? await _appointmentService.GetAppointmentsByDoctorAsync(userId)
            : await _appointmentService.GetAppointmentsByPatientAsync(userId);
    }
}
