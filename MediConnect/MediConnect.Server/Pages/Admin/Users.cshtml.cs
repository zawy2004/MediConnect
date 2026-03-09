using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Admin;

public class UsersModel : PageModel
{
    private readonly MediconnectContext _context;

    public UsersModel(MediconnectContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
    [BindProperty(SupportsGet = true)] public string? RoleFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "all";

    public List<User> Users { get; set; } = new();
    public List<DoctorProfile> PendingDoctors { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");
        if (userId == null || role != "ADMIN") return RedirectToPage("/Auth/Login");

        if (Tab == "pending")
        {
            var query = _context.DoctorProfiles
                .Include(d => d.User)
                .Where(d => d.ApprovalStatus == "PENDING");

            if (!string.IsNullOrEmpty(SearchTerm))
                query = query.Where(d => d.User.FullName.Contains(SearchTerm) || d.User.Email.Contains(SearchTerm));

            PendingDoctors = await query.OrderByDescending(d => d.User.CreatedAt).ToListAsync();
        }
        else
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
                query = query.Where(u => u.FullName.Contains(SearchTerm) || u.Email.Contains(SearchTerm));

            if (!string.IsNullOrEmpty(RoleFilter))
                query = query.Where(u => u.Role.RoleName == RoleFilter);

            Users = await query.OrderByDescending(u => u.CreatedAt).Take(50).ToListAsync();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int doctorProfileId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var profile = await _context.DoctorProfiles.FindAsync(doctorProfileId);
        if (profile != null)
        {
            profile.ApprovalStatus = "APPROVED";
            profile.ApprovedBy = userId;
            profile.ApprovedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage(new { Tab = "pending" });
    }

    public async Task<IActionResult> OnPostRejectAsync(int doctorProfileId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var profile = await _context.DoctorProfiles.FindAsync(doctorProfileId);
        if (profile != null)
        {
            profile.ApprovalStatus = "REJECTED";
            profile.ApprovedBy = userId;
            profile.ApprovedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage(new { Tab = "pending" });
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
