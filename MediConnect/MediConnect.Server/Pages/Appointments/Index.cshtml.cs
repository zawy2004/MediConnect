using MediConnect.Server.Data;
using MediConnect.Server.Models;
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

    public List<Appointment> Appointments { get; set; } = new();

    public string GetBadgeClass(string status)
    {
        if (status == "PENDING") return "badge-pending";
        if (status == "CONFIRMED") return "badge-confirmed";
        if (status == "COMPLETED") return "badge-completed";
        if (status.StartsWith("CANCELLED")) return "badge-cancelled";
        return "bg-secondary";
    }

    public async Task OnGetAsync()
    {
        // TODO: Replace with actual logged-in user ID from authentication
        // For now, load all appointments for demo
        Appointments = await _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Include(a => a.Specialty)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.StartTime)
            .ToListAsync();
    }
}
