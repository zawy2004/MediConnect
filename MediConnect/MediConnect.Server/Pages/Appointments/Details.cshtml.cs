using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Appointments;

public class DetailsModel : PageModel
{
    private readonly MediconnectContext _context;

    public DetailsModel(MediconnectContext context)
    {
        _context = context;
    }

    public Appointment? Appointment { get; set; }

    public string GetBadgeClass(string status)
    {
        if (status == "PENDING") return "badge-pending";
        if (status == "CONFIRMED") return "badge-confirmed";
        if (status == "COMPLETED") return "badge-completed";
        if (status.StartsWith("CANCELLED")) return "badge-cancelled";
        return "bg-secondary";
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Include(a => a.Specialty)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);

        if (Appointment == null) return NotFound();

        return Page();
    }
}
