using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Admin;

public class ReportsModel : PageModel
{
    private readonly MediconnectContext _context;

    public ReportsModel(MediconnectContext context)
    {
        _context = context;
    }

    public int TotalVisits { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal AverageSatisfaction { get; set; }
    public List<DoctorPerformance> DoctorPerformances { get; set; } = new();

    public class DoctorPerformance
    {
        public string DoctorName { get; set; } = "";
        public string? Specialty { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal? AverageRating { get; set; }
        public int NoShowCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");
        if (userId == null || role != "ADMIN") return RedirectToPage("/Auth/Login");

        var allAppointments = await _context.Appointments.ToListAsync();
        TotalVisits = allAppointments.Count;
        var completed = allAppointments.Count(a => a.Status == "COMPLETED");
        CompletionRate = TotalVisits > 0 ? (decimal)completed / TotalVisits * 100 : 0;

        TotalRevenue = await _context.Payments
            .Where(p => p.PaymentStatus == "COMPLETED")
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        AverageSatisfaction = (decimal)(await _context.Reviews
            .Where(r => r.IsVisible)
            .AverageAsync(r => (double?)r.Rating) ?? 0);

        // Doctor performance
        var doctors = await _context.DoctorProfiles
            .Include(d => d.User)
            .Where(d => d.ApprovalStatus == "APPROVED")
            .ToListAsync();

        var appointmentsByDoctor = allAppointments
            .GroupBy(a => a.DoctorId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var ratingsByDoctor = await _context.Reviews
            .Where(r => r.IsVisible)
            .GroupBy(r => r.DoctorId)
            .Select(g => new { DoctorId = g.Key, Avg = g.Average(r => (double)r.Rating) })
            .ToDictionaryAsync(g => g.DoctorId, g => g.Avg);

        var primarySpecialties = await _context.DoctorSpecialties
            .Include(ds => ds.Specialty)
            .Where(ds => ds.IsPrimary == true)
            .ToDictionaryAsync(ds => ds.UserId, ds => ds.Specialty.SpecialtyName);

        DoctorPerformances = doctors.Select(d =>
        {
            var appts = appointmentsByDoctor.GetValueOrDefault(d.UserId, new List<Appointment>());
            var completedCount = appts.Count(a => a.Status == "COMPLETED");
            var cancelledCount = appts.Count(a => a.Status == "CANCELLED");
            return new DoctorPerformance
            {
                DoctorName = d.User.FullName,
                Specialty = primarySpecialties.GetValueOrDefault(d.UserId),
                TotalAppointments = appts.Count,
                CompletedAppointments = completedCount,
                CompletionRate = appts.Count > 0 ? (decimal)completedCount / appts.Count * 100 : 0,
                AverageRating = ratingsByDoctor.TryGetValue(d.UserId, out var avg) ? (decimal)avg : null,
                NoShowCount = cancelledCount,
                Revenue = d.ConsultationFee * completedCount
            };
        })
        .OrderByDescending(d => d.CompletionRate)
        .Take(20)
        .ToList();

        return Page();
    }
}
