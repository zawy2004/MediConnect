using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Admin;

public class SystemModel : PageModel
{
    private readonly MediconnectContext _context;

    public SystemModel(MediconnectContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }
    [BindProperty(SupportsGet = true)] public string? SeverityFilter { get; set; }

    public List<SystemLog> Logs { get; set; } = new();
    public int TotalLogs { get; set; }
    public int InfoCount { get; set; }
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");
        if (userId == null || role != "ADMIN") return RedirectToPage("/Auth/Login");

        var allLogs = _context.SystemLogs.AsQueryable();
        TotalLogs = await allLogs.CountAsync();
        InfoCount = await allLogs.CountAsync(l => l.Severity == "INFO");
        WarningCount = await allLogs.CountAsync(l => l.Severity == "WARNING");
        ErrorCount = await allLogs.CountAsync(l => l.Severity == "ERROR");

        var query = _context.SystemLogs.Include(l => l.User).AsQueryable();

        if (!string.IsNullOrEmpty(SearchTerm))
            query = query.Where(l => l.Action.Contains(SearchTerm) || (l.Description != null && l.Description.Contains(SearchTerm)));

        if (!string.IsNullOrEmpty(SeverityFilter))
            query = query.Where(l => l.Severity == SeverityFilter);

        Logs = await query.OrderByDescending(l => l.CreatedAt).Take(100).ToListAsync();
        return Page();
    }
}
