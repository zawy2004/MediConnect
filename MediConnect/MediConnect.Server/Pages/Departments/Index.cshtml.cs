using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Departments;

public class IndexModel : PageModel
{
    private readonly MediconnectContext _context;

    public IndexModel(MediconnectContext context)
    {
        _context = context;
    }

    public List<Department> Departments { get; set; } = new();

    public async Task OnGetAsync()
    {
        Departments = await _context.Departments
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();
    }
}
