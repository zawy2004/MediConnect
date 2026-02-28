using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Doctors;

public class DetailsModel : PageModel
{
    private readonly MediconnectContext _context;

    public DetailsModel(MediconnectContext context)
    {
        _context = context;
    }

    public DoctorProfile? Doctor { get; set; }
    public List<Review> Reviews { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Doctor = await _context.DoctorProfiles
            .Include(d => d.User)
                .ThenInclude(u => u.DoctorSpecialties)
                    .ThenInclude(ds => ds.Specialty)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.DoctorProfileId == id);

        if (Doctor == null) return NotFound();

        Reviews = await _context.Reviews
            .Where(r => r.DoctorId == Doctor.UserId && r.IsVisible)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .ToListAsync();

        return Page();
    }
}
