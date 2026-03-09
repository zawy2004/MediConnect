using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Doctor;

public class PerformanceModel : PageModel
{
    private readonly MediconnectContext _context;

    public PerformanceModel(MediconnectContext context)
    {
        _context = context;
    }

    public DoctorProfile? DoctorProfile { get; set; }
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<Review> RecentReviews { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        DoctorProfile = await _context.DoctorProfiles
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (DoctorProfile == null) return RedirectToPage("/Auth/Login");

        var appointments = await _context.Appointments
            .Where(a => a.DoctorId == userId)
            .ToListAsync();

        TotalAppointments = appointments.Count;
        CompletedAppointments = appointments.Count(a => a.Status == "COMPLETED");
        CancelledAppointments = appointments.Count(a => a.Status == "CANCELLED");
        CompletionRate = TotalAppointments > 0 ? (decimal)CompletedAppointments / TotalAppointments * 100 : 0;

        var completedAppointmentIds = appointments
            .Where(a => a.Status == "COMPLETED")
            .Select(a => a.AppointmentId)
            .ToList();
        TotalRevenue = await _context.Payments
            .Where(p => completedAppointmentIds.Contains(p.AppointmentId) && p.PaymentStatus == "COMPLETED")
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        RecentReviews = await _context.Reviews
            .Include(r => r.Patient)
            .Where(r => r.DoctorId == userId && r.IsVisible)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync();

        return Page();
    }
}
