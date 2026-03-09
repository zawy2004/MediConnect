using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Appointments;

public class AppointmentListModel : PageModel
{
    private readonly MediconnectContext _context;

    public AppointmentListModel(MediconnectContext context)
    {
        _context = context;
    }

    public List<Appointment> UpcomingAppointments { get; set; } = new();
    public List<Appointment> PastAppointments { get; set; } = new();
    public string UserRole { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        UserRole = HttpContext.Session.GetString("UserRole") ?? "";
        var today = DateOnly.FromDateTime(DateTime.Today);

        var query = _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Include(a => a.Specialty)
            .AsQueryable();

        if (UserRole == "DOCTOR")
            query = query.Where(a => a.DoctorId == userId);
        else
            query = query.Where(a => a.PatientId == userId);

        UpcomingAppointments = await query
            .Where(a => a.AppointmentDate >= today && a.Status != "CANCELLED")
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
            .ToListAsync();

        PastAppointments = await query
            .Where(a => a.AppointmentDate < today || a.Status == "CANCELLED")
            .OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime)
            .Take(20)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int appointmentId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var appt = await _context.Appointments.FindAsync(appointmentId);
        if (appt == null) return NotFound();
        if (appt.PatientId != userId && appt.DoctorId != userId) return Forbid();

        appt.Status = "CANCELLED";
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }
}
