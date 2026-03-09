using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Patient;

public class IndexModel : PageModel
{
    private readonly MediconnectContext _context;

    public IndexModel(MediconnectContext context)
    {
        _context = context;
    }

    public User? CurrentUser { get; set; }
    public List<Appointment> UpcomingAppointments { get; set; } = new();
    public List<AiRecommendation> AiRecommendations { get; set; } = new();
    public List<PromotionalCampaign> Promotions { get; set; } = new();
    public List<DoctorProfile> FeaturedDoctors { get; set; } = new();
    public List<Specialty> SuggestedSpecialties { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // TODO: Replace with real auth
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        CurrentUser = await _context.Users.FindAsync(userId.Value);
        if (CurrentUser == null) return RedirectToPage("/Auth/Login");

        var today = DateOnly.FromDateTime(DateTime.Today);

        UpcomingAppointments = await _context.Appointments
            .Where(a => a.PatientId == userId.Value && a.AppointmentDate >= today)
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
            .Take(10)
            .ToListAsync();

        AiRecommendations = await _context.AiRecommendations
            .Where(r => r.PatientId == userId.Value)
            .OrderByDescending(r => r.CreatedAt)
            .Take(4)
            .ToListAsync();

        Promotions = await _context.PromotionalCampaigns
            .Where(p => p.TargetRole == "PATIENT" && p.IsSent)
            .OrderByDescending(p => p.CreatedAt)
            .Take(6)
            .ToListAsync();

        FeaturedDoctors = await _context.DoctorProfiles
            .Include(d => d.User)
            .Include(d => d.Department)
            .Where(d => d.ApprovalStatus == "APPROVED")
            .OrderByDescending(d => d.AverageRating)
            .Take(4)
            .ToListAsync();

        SuggestedSpecialties = await _context.Specialties
            .Where(s => s.IsActive)
            .OrderBy(s => s.SpecialtyName)
            .Take(4)
            .ToListAsync();

        return Page();
    }
}
