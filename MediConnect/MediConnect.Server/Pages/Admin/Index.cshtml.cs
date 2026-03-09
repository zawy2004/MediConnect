using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly MediconnectContext _context;

    public IndexModel(MediconnectContext context)
    {
        _context = context;
    }

    public int TotalUsers { get; set; }
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TodayAppointments { get; set; }
    public int ProcessingAppointments { get; set; }
    public decimal SystemUptime { get; set; } = 99.9m;
    public List<DoctorProfile> PendingDoctors { get; set; } = new();
    public List<SystemLog> RecentLogs { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");
        if (userId == null || role != "ADMIN") return RedirectToPage("/Auth/Login");

        TotalUsers = await _context.Users.CountAsync(u => u.IsActive);
        TotalPatients = await _context.Users.CountAsync(u => u.IsActive && u.Role.RoleName == "PATIENT");
        TotalDoctors = await _context.Users.CountAsync(u => u.IsActive && u.Role.RoleName == "DOCTOR");

        var today = DateOnly.FromDateTime(DateTime.Today);
        TodayAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate == today);
        ProcessingAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate == today && a.Status == "PENDING");

        PendingDoctors = await _context.DoctorProfiles
            .Include(d => d.User)
            .Where(d => d.ApprovalStatus == "PENDING")
            .OrderByDescending(d => d.User.CreatedAt)
            .Take(5)
            .ToListAsync();

        RecentLogs = await _context.SystemLogs
            .Include(l => l.User)
            .OrderByDescending(l => l.CreatedAt)
            .Take(5)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int doctorProfileId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var profile = await _context.DoctorProfiles.FindAsync(doctorProfileId);
        if (profile != null)
        {
            profile.ApprovalStatus = "APPROVED";
            profile.ApprovedBy = userId;
            profile.ApprovedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int doctorProfileId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var profile = await _context.DoctorProfiles.FindAsync(doctorProfileId);
        if (profile != null)
        {
            profile.ApprovalStatus = "REJECTED";
            profile.ApprovedBy = userId;
            profile.ApprovedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
