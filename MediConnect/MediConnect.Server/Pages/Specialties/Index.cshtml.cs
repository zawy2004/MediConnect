using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Specialties;

public class IndexModel : PageModel
{
    private readonly MediconnectContext _context;

    public IndexModel(MediconnectContext context)
    {
        _context = context;
    }

    public List<Specialty> Specialties { get; set; } = new();

    public async Task OnGetAsync()
    {
        Specialties = await _context.Specialties
            .Where(s => s.IsActive)
            .OrderBy(s => s.SpecialtyName)
            .ToListAsync();
    }
}
