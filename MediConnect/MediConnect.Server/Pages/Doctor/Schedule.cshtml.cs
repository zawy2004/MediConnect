using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Doctor;

public class ScheduleModel : PageModel
{
    private readonly MediconnectContext _context;

    public ScheduleModel(MediconnectContext context)
    {
        _context = context;
    }

    public List<DoctorSchedule> WeeklySchedules { get; set; } = new();
    public List<TimeSlot> UpcomingSlots { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        WeeklySchedules = await _context.DoctorSchedules
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        UpcomingSlots = await _context.TimeSlots
            .Include(s => s.Appointments).ThenInclude(a => a.Patient)
            .Where(s => s.UserId == userId && s.SlotDate >= today)
            .OrderBy(s => s.SlotDate).ThenBy(s => s.StartTime)
            .Take(30)
            .ToListAsync();

        return Page();
    }
}
