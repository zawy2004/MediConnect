using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Admin;

public class ComplaintsModel : PageModel
{
    private readonly MediconnectContext _context;

    public ComplaintsModel(MediconnectContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
    [BindProperty(SupportsGet = true)] public string? StatusFilter { get; set; }

    public List<Complaint> Complaints { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");
        if (userId == null || role != "ADMIN") return RedirectToPage("/Auth/Login");

        var query = _context.Complaints
            .Include(c => c.Patient)
            .Include(c => c.Doctor)
            .AsQueryable();

        if (!string.IsNullOrEmpty(SearchTerm))
            query = query.Where(c => c.Subject.Contains(SearchTerm) || c.Patient.FullName.Contains(SearchTerm));

        if (!string.IsNullOrEmpty(StatusFilter))
            query = query.Where(c => c.Status == StatusFilter);

        Complaints = await query.OrderByDescending(c => c.CreatedAt).Take(50).ToListAsync();
        return Page();
    }

    [BindProperty] public string? ResolutionNote { get; set; }

    public async Task<IActionResult> OnPostResolveAsync(int complaintId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var complaint = await _context.Complaints.FindAsync(complaintId);
        if (complaint != null)
        {
            complaint.Status = "RESOLVED";
            complaint.ResolvedBy = userId;
            complaint.ResolvedAt = DateTime.Now;
            complaint.ResolutionNote = ResolutionNote;
            complaint.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDismissAsync(int complaintId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var complaint = await _context.Complaints.FindAsync(complaintId);
        if (complaint != null)
        {
            complaint.Status = "DISMISSED";
            complaint.ResolvedBy = userId;
            complaint.ResolvedAt = DateTime.Now;
            complaint.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
