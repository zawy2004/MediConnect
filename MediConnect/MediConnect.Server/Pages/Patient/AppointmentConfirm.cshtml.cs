using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class AppointmentConfirmModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;

    public AppointmentConfirmModel(IAppointmentService appointmentService, IDoctorService doctorService)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
    }

    [BindProperty]
    [Required]
    public int DoctorUserId { get; set; }

    [BindProperty]
    [Required]
    public int SlotId { get; set; }

    [BindProperty]
    public int? SpecialtyId { get; set; }

    [BindProperty]
    [Required]
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

    [BindProperty]
    public string? Reason { get; set; }

    public DoctorListDto? Doctor { get; set; }
    public SelectList SlotList { get; set; } = default!;
    public SelectList SpecialtyList { get; set; } = default!;
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int doctorUserId)
    {
        DoctorUserId = doctorUserId;
        await LoadData();
        if (Doctor == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadData();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _appointmentService.CreateAppointmentAsync(new CreateAppointmentDto
        {
            PatientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            DoctorId = DoctorUserId,
            SlotId = SlotId,
            SpecialtyId = SpecialtyId,
            AppointmentDate = AppointmentDate,
            Reason = Reason
        });

        if (!result.Success || !result.AppointmentId.HasValue)
        {
            ErrorMessage = result.ErrorMessage ?? "Không thể tạo lịch hẹn.";
            return Page();
        }

        return RedirectToPage("/Patient/AppointmentSuccess", new { appointmentId = result.AppointmentId.Value });
    }

    private async Task LoadData()
    {
        var doctors = await _doctorService.SearchDoctorsAsync(new DoctorSearchFilterDto());
        Doctor = doctors.FirstOrDefault(d => d.UserId == DoctorUserId);

        var slots = await _appointmentService.GetAvailableSlotsAsync();
        SlotList = new SelectList(slots, "SlotId", "Display");

        var specialties = await _doctorService.GetActiveSpecialtiesAsync();
        SpecialtyList = new SelectList(specialties, "SpecialtyId", "SpecialtyName");
    }
}
