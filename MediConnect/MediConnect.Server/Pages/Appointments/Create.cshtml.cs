using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediConnect.Server.Pages.Appointments;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;

    public CreateModel(IAppointmentService appointmentService, IDoctorService doctorService)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
    }

    [BindProperty]
    public int? SpecialtyId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
    public int DoctorId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng chọn ngày hẹn")]
    [DataType(DataType.Date)]
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng chọn khung giờ")]
    public int SlotId { get; set; }

    [BindProperty]
    public string? Reason { get; set; }

    public SelectList SpecialtyList { get; set; } = default!;
    public SelectList DoctorList { get; set; } = default!;
    public SelectList SlotList { get; set; } = default!;
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task OnGetAsync(int? doctorId)
    {
        await LoadDropdowns();
        if (doctorId.HasValue)
        {
            DoctorId = doctorId.Value;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDropdowns();

        if (!ModelState.IsValid) return Page();

        var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _appointmentService.CreateAppointmentAsync(new CreateAppointmentDto
        {
            PatientId = patientId,
            DoctorId = DoctorId,
            SlotId = SlotId,
            SpecialtyId = SpecialtyId,
            AppointmentDate = AppointmentDate,
            Reason = Reason
        });

        if (!result.Success)
        {
            ErrorMessage = result.ErrorMessage;
            return Page();
        }

        SuccessMessage = "Đặt lịch hẹn thành công! Vui lòng chờ bác sĩ xác nhận.";
        return Page();
    }

    private async Task LoadDropdowns()
    {
        var specialties = await _doctorService.GetActiveSpecialtiesAsync();
        SpecialtyList = new SelectList(specialties, "SpecialtyId", "SpecialtyName");

        var doctors = await _doctorService.SearchDoctorsAsync(new DoctorSearchFilterDto());
        DoctorList = new SelectList(doctors, "UserId", "FullName");

        // Load slots của bác sĩ đã chọn (nếu có)
        if (DoctorId > 0)
        {
            var fromDate = DateOnly.FromDateTime(DateTime.Today);
            var toDate = fromDate.AddDays(30);
            var slots = await _appointmentService.GetAvailableSlotsByDoctorAsync(DoctorId, fromDate, toDate);
            SlotList = new SelectList(slots, "SlotId", "Display");
        }
        else
        {
            SlotList = new SelectList(Enumerable.Empty<TimeSlotDto>(), "SlotId", "Display");
        }
    }
}
