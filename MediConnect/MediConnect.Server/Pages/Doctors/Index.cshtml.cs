using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Doctors;

public class IndexModel : PageModel
{
    private readonly MediconnectContext _context;

    public IndexModel(MediconnectContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SpecialtyId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DepartmentId { get; set; }

    public List<DoctorProfile> Doctors { get; set; } = new();
    public SelectList SpecialtyList { get; set; } = default!;
    public SelectList DepartmentList { get; set; } = default!;

    public async Task OnGetAsync()
    {
        // Load filter dropdowns
        var specialties = await _context.Specialties.Where(s => s.IsActive).OrderBy(s => s.SpecialtyName).ToListAsync();
        SpecialtyList = new SelectList(specialties, "SpecialtyId", "SpecialtyName");

        var departments = await _context.Departments.Where(d => d.IsActive).OrderBy(d => d.DepartmentName).ToListAsync();
        DepartmentList = new SelectList(departments, "DepartmentId", "DepartmentName");

        // Build query
        var query = _context.DoctorProfiles
            .Include(d => d.User)
                .ThenInclude(u => u.DoctorSpecialties)
                    .ThenInclude(ds => ds.Specialty)
            .Where(d => d.ApprovalStatus == "APPROVED");

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(d => d.User.FullName.Contains(SearchTerm));
        }

        if (SpecialtyId.HasValue)
        {
            query = query.Where(d => d.User.DoctorSpecialties.Any(ds => ds.SpecialtyId == SpecialtyId));
        }

        if (DepartmentId.HasValue)
        {
            query = query.Where(d => d.DepartmentId == DepartmentId);
        }

        Doctors = await query.OrderBy(d => d.User.FullName).ToListAsync();
    }
}
