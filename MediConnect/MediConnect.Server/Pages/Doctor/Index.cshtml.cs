using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Doctor;

public class IndexModel : PageModel
{
    private readonly MediconnectContext _context;

    public IndexModel(MediconnectContext context)
    {
        _context = context;
    }

    public DoctorProfile? DoctorProfile { get; set; }
    public int TotalPatients { get; set; }
    public int MonthlyAppointments { get; set; }
    public List<Appointment> TodayAppointments { get; set; } = new();
    public List<Appointment> PendingAppointments { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        DoctorProfile = await _context.DoctorProfiles
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (DoctorProfile == null) return RedirectToPage("/Auth/Login");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        TotalPatients = await _context.Appointments
            .Where(a => a.DoctorId == userId)
            .Select(a => a.PatientId).Distinct().CountAsync();

        MonthlyAppointments = await _context.Appointments
            .Where(a => a.DoctorId == userId && a.AppointmentDate >= monthStart)
            .CountAsync();

        TodayAppointments = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Specialty)
            .Where(a => a.DoctorId == userId && a.AppointmentDate == today)
            .OrderBy(a => a.StartTime)
            .ToListAsync();

        PendingAppointments = await _context.Appointments
            .Include(a => a.Patient)
            .Where(a => a.DoctorId == userId && a.Status == "PENDING")
            .OrderBy(a => a.AppointmentDate)
            .Take(5)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int appointmentId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var appt = await _context.Appointments.FindAsync(appointmentId);
        if (appt != null && appt.DoctorId == userId)
        {
            appt.Status = "CONFIRMED";
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int appointmentId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var appt = await _context.Appointments.FindAsync(appointmentId);
        if (appt != null && appt.DoctorId == userId)
        {
            appt.Status = "CANCELLED";
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
