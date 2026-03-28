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
public class DashboardModel : PageModel
{
    private readonly IPatientPortalService _patientPortalService;
    private readonly IAppointmentService _appointmentService;
    private readonly IAppointmentRepository _appointmentRepository;

    public DashboardModel(
        IPatientPortalService patientPortalService,
        IAppointmentService appointmentService,
        IAppointmentRepository appointmentRepository)
    {
        _patientPortalService = patientPortalService;
        _appointmentService = appointmentService;
        _appointmentRepository = appointmentRepository;
    }

    public PatientPortalDashboardDto Data { get; set; } = new();

    [BindProperty(SupportsGet = true, Name = "p")]
    public int P { get; set; } = 1;

    public string GetBadgeClass(string status) => StatusHelper.GetBadgeClass(status);

    public async Task OnGetAsync()
    {
        Data = await _patientPortalService.GetDashboardAsync(GetUserId(), P, 4);
    }

    public async Task<IActionResult> OnPostCancelAsync(int appointmentId)
    {
        var patientId = GetUserId();
        var appt = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appt == null || appt.PatientId != patientId)
            return RedirectToPage();

        await _appointmentService.CancelAppointmentAsync(new CancelAppointmentDto
        {
            AppointmentId = appointmentId,
            CancelReason = "Hủy từ dashboard bệnh nhân"
        });

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelAjaxAsync(int appointmentId, int schedulePage = 1)
    {
        var patientId = GetUserId();
        var appt = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appt == null || appt.PatientId != patientId)
            return new JsonResult(new { success = false, error = "Không tìm thấy lịch hẹn." });

        var result = await _appointmentService.CancelAppointmentAsync(new CancelAppointmentDto
        {
            AppointmentId = appointmentId,
            CancelReason = "Hủy từ dashboard bệnh nhân"
        });

        if (!result.Success)
            return new JsonResult(new { success = false, error = result.ErrorMessage ?? "Không thể hủy lịch." });

        var data = await _patientPortalService.GetDashboardAsync(patientId, schedulePage, 4);
        var totalPages = Math.Max(1, (int)Math.Ceiling(data.ScheduleTotalCount / (double)Math.Max(1, data.SchedulePageSize)));
        return new JsonResult(new
        {
            success = true,
            appointmentId,
            upcoming = data.UpcomingAppointments,
            completed = data.CompletedAppointments,
            cancelled = data.CancelledAppointments,
            scheduleTotalCount = data.ScheduleTotalCount,
            schedulePage = data.SchedulePage,
            schedulePageSize = data.SchedulePageSize,
            scheduleTotalPages = totalPages
        });
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
