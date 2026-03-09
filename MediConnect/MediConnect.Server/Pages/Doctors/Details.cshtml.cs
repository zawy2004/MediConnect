using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Doctors;

public class DetailsModel : PageModel
{
    private readonly MediconnectContext _context;

    public DetailsModel(MediconnectContext context)
    {
        _context = context;
    }

    public DoctorProfile? Doctor { get; set; }
    public List<Review> Reviews { get; set; } = new();
    public List<TimeSlot> AvailableSlots { get; set; } = new();

    [BindProperty]
    public int? SelectedSlotId { get; set; }

    [BindProperty]
    public string? Reason { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Doctor = await _context.DoctorProfiles
            .Include(d => d.User)
                .ThenInclude(u => u.DoctorSpecialties)
                    .ThenInclude(ds => ds.Specialty)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.DoctorProfileId == id);

        if (Doctor == null) return NotFound();

        Reviews = await _context.Reviews
            .Include(r => r.Patient)
            .Where(r => r.DoctorId == Doctor.UserId && r.IsVisible)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .ToListAsync();

        AvailableSlots = await _context.TimeSlots
            .Where(s => s.UserId == Doctor.UserId && s.IsAvailable && s.SlotDate >= DateOnly.FromDateTime(DateTime.Today))
            .OrderBy(s => s.SlotDate).ThenBy(s => s.StartTime)
            .Take(12)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostBookAsync(int id)
    {
        var patientId = HttpContext.Session.GetInt32("UserId");
        if (patientId == null) return RedirectToPage("/Auth/Login");

        if (!SelectedSlotId.HasValue) return RedirectToPage("/Doctors/Details", new { id });

        var slot = await _context.TimeSlots.FindAsync(SelectedSlotId.Value);
        if (slot == null || !slot.IsAvailable) return RedirectToPage("/Doctors/Details", new { id });

        var doctor = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.DoctorProfileId == id);
        if (doctor == null) return NotFound();

        var appointment = new Appointment
        {
            PatientId = patientId.Value,
            DoctorId = doctor.UserId,
            SlotId = slot.SlotId,
            AppointmentDate = slot.SlotDate,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime,
            Reason = Reason,
            Status = "CONFIRMED"
        };

        _context.Appointments.Add(appointment);
        slot.BookedCount++;
        if (slot.BookedCount >= slot.MaxCapacity) slot.IsAvailable = false;

        await _context.SaveChangesAsync();
        return RedirectToPage("/Appointments/Index");
    }
}
