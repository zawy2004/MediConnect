using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Doctor;

public class PatientsModel : PageModel
{
    private readonly MediconnectContext _context;

    public PatientsModel(MediconnectContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public List<PatientInfo> Patients { get; set; } = new();

    public class PatientInfo
    {
        public User Patient { get; set; } = null!;
        public int VisitCount { get; set; }
        public DateOnly? LastVisit { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var query = _context.Appointments
            .Where(a => a.DoctorId == userId)
            .GroupBy(a => a.PatientId)
            .Select(g => new
            {
                PatientId = g.Key,
                VisitCount = g.Count(),
                LastVisit = g.Max(a => a.AppointmentDate)
            });

        var patientGroups = await query.ToListAsync();
        var patientIds = patientGroups.Select(p => p.PatientId).ToList();
        var users = await _context.Users.Where(u => patientIds.Contains(u.UserId)).ToDictionaryAsync(u => u.UserId);

        Patients = patientGroups
            .Where(p => users.ContainsKey(p.PatientId))
            .Select(p => new PatientInfo
            {
                Patient = users[p.PatientId],
                VisitCount = p.VisitCount,
                LastVisit = p.LastVisit
            })
            .OrderByDescending(p => p.LastVisit)
            .ToList();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            Patients = Patients.Where(p => p.Patient.FullName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return Page();
    }
}
