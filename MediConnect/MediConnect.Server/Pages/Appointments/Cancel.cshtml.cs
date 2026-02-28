using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Appointments;

public class CancelModel : PageModel
{
    private readonly MediconnectContext _context;

    public CancelModel(MediconnectContext context)
    {
        _context = context;
    }

    [BindProperty]
    public int AppointmentId { get; set; }

    [BindProperty]
    public string? CancelReason { get; set; }

    public Appointment? Appointment { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Appointment = await _context.Appointments
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);

        if (Appointment == null) return NotFound();

        AppointmentId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var appointment = await _context.Appointments
            .Include(a => a.Slot)
            .FirstOrDefaultAsync(a => a.AppointmentId == AppointmentId);

        if (appointment == null) return NotFound();

        appointment.Status = "CANCELLED_BY_PATIENT";
        appointment.CancelReason = CancelReason;
        appointment.CancelledAt = DateTime.Now;
        appointment.UpdatedAt = DateTime.Now;

        // Free up the time slot
        if (appointment.Slot != null)
        {
            appointment.Slot.BookedCount = Math.Max(0, appointment.Slot.BookedCount - 1);
            if (appointment.Slot.BookedCount < appointment.Slot.MaxCapacity)
            {
                appointment.Slot.IsAvailable = true;
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToPage("/Appointments/Index");
    }
}
