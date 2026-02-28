using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages;

public class IndexModel : PageModel
{
    private readonly MediconnectContext _context;

    public IndexModel(MediconnectContext context)
    {
        _context = context;
    }

    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TotalAppointments { get; set; }
    public int TotalSpecialties { get; set; }
    public List<Specialty> Specialties { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalPatients = await _context.Users.CountAsync(u => u.Role.RoleName == "PATIENT");
        TotalDoctors = await _context.Users.CountAsync(u => u.Role.RoleName == "DOCTOR");
        TotalAppointments = await _context.Appointments.CountAsync();
        TotalSpecialties = await _context.Specialties.CountAsync(s => s.IsActive);
        Specialties = await _context.Specialties
            .Where(s => s.IsActive)
            .OrderBy(s => s.SpecialtyName)
            .Take(8)
            .ToListAsync();
    }
}
