using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class DoctorProfileModel : PageModel
{
    private readonly IPatientPortalService _patientPortalService;
    private readonly IAppointmentService _appointmentService;

    public DoctorProfileModel(IPatientPortalService patientPortalService, IAppointmentService appointmentService)
    {
        _patientPortalService = patientPortalService;
        _appointmentService = appointmentService;
    }

    [BindProperty(SupportsGet = true)]
    public int DoctorProfileId { get; set; }

    [BindProperty, Required]
    public int SlotId { get; set; }

    [BindProperty]
    public string? Reason { get; set; }

    public PatientDoctorProfileDto? Data { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (DoctorProfileId <= 0)
        {
            return RedirectToPage("/Patient/SearchDoctors");
        }

        Data = await _patientPortalService.GetDoctorProfileAsync(DoctorProfileId);
        if (Data == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostBookAsync()
    {
        Data = await _patientPortalService.GetDoctorProfileAsync(DoctorProfileId);
        if (Data?.Doctor == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var slot = Data.NextSlots.FirstOrDefault(s => s.SlotId == SlotId);
        if (slot == null)
        {
            ErrorMessage = "Khung giờ không hợp lệ hoặc đã hết chỗ.";
            return Page();
        }

        var created = await _appointmentService.CreateAppointmentAsync(new CreateAppointmentDto
        {
            PatientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            DoctorId = Data.Doctor.UserId,
            SlotId = SlotId,
            AppointmentDate = slot.SlotDate.ToDateTime(TimeOnly.MinValue),
            SpecialtyId = null,
            Reason = Reason
        });

        if (!created.Success || !created.AppointmentId.HasValue)
        {
            ErrorMessage = created.ErrorMessage ?? "Không thể đặt lịch.";
            return Page();
        }

        return RedirectToPage("/Patient/PaymentConfirm", new { appointmentId = created.AppointmentId.Value });
    }
}
