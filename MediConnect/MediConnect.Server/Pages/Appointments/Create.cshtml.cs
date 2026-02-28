using System.ComponentModel.DataAnnotations;
using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Appointments;

public class CreateModel : PageModel
{
    private readonly MediconnectContext _context;

    public CreateModel(MediconnectContext context)
    {
        _context = context;
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

        var slot = await _context.TimeSlots.FindAsync(SlotId);
        if (slot == null || !slot.IsAvailable || slot.BookedCount >= slot.MaxCapacity)
        {
            ErrorMessage = "Khung giờ này không còn trống.";
            return Page();
        }

        // TODO: Get actual patient ID from authentication
        var appointment = new Appointment
        {
            PatientId = 1, // Placeholder — replace with auth user
            DoctorId = DoctorId,
            SlotId = SlotId,
            SpecialtyId = SpecialtyId,
            AppointmentDate = DateOnly.FromDateTime(AppointmentDate),
            StartTime = slot.StartTime,
            EndTime = slot.EndTime,
            Reason = Reason,
            Status = "PENDING",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Appointments.Add(appointment);

        // Update slot booked count
        slot.BookedCount++;
        if (slot.BookedCount >= slot.MaxCapacity)
        {
            slot.IsAvailable = false;
        }

        await _context.SaveChangesAsync();

        SuccessMessage = "Đặt lịch hẹn thành công! Vui lòng chờ bác sĩ xác nhận.";
        return Page();
    }

    private async Task LoadDropdowns()
    {
        var specialties = await _context.Specialties.Where(s => s.IsActive).OrderBy(s => s.SpecialtyName).ToListAsync();
        SpecialtyList = new SelectList(specialties, "SpecialtyId", "SpecialtyName");

        var doctors = await _context.DoctorProfiles
            .Include(d => d.User)
            .Where(d => d.ApprovalStatus == "APPROVED")
            .Select(d => new { d.UserId, d.User.FullName })
            .ToListAsync();
        DoctorList = new SelectList(doctors, "UserId", "FullName");

        var slotsRaw = await _context.TimeSlots
            .Where(s => s.IsAvailable && s.SlotDate >= DateOnly.FromDateTime(DateTime.Today))
            .OrderBy(s => s.SlotDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
        SlotList = new SelectList(
            slotsRaw.Select(s => new
            {
                s.SlotId,
                Display = s.SlotDate.ToString("dd/MM") + " | " + s.StartTime.ToString(@"hh\:mm") + " - " + s.EndTime.ToString(@"hh\:mm")
            }),
            "SlotId", "Display");
    }
}
