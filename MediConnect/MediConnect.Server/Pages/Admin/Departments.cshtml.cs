using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Admin;

public class DepartmentsModel : PageModel
{
    private readonly MediconnectContext _context;

    public DepartmentsModel(MediconnectContext context)
    {
        _context = context;
    }

    [BindProperty(SupportsGet = true)] public string? SearchTerm { get; set; }

    public List<SpecialtyInfo> Specialties { get; set; } = new();
    public List<Department> AllDepartments { get; set; } = new();

    public class SpecialtyInfo
    {
        public Specialty Specialty { get; set; } = null!;
        public int DoctorCount { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");
        if (userId == null || role != "ADMIN") return RedirectToPage("/Auth/Login");

        var query = _context.Specialties.AsQueryable();
        if (!string.IsNullOrEmpty(SearchTerm))
            query = query.Where(s => s.SpecialtyName.Contains(SearchTerm));

        var specialties = await query.OrderBy(s => s.SpecialtyName).ToListAsync();

        var doctorCounts = await _context.DoctorSpecialties
            .GroupBy(ds => ds.SpecialtyId)
            .Select(g => new { SpecialtyId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.SpecialtyId, g => g.Count);

        Specialties = specialties.Select(s => new SpecialtyInfo
        {
            Specialty = s,
            DoctorCount = doctorCounts.GetValueOrDefault(s.SpecialtyId, 0)
        }).ToList();

        AllDepartments = await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync();

        return Page();
    }

    [BindProperty] public string NewSpecialtyName { get; set; } = "";
    [BindProperty] public string? NewDescription { get; set; }

    public async Task<IActionResult> OnPostAddAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var specialty = new Specialty
        {
            SpecialtyName = NewSpecialtyName,
            Description = NewDescription,
            IsActive = true
        };
        _context.Specialties.Add(specialty);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Thêm chuyên khoa thành công!";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int specialtyId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var specialty = await _context.Specialties.FindAsync(specialtyId);
        if (specialty != null)
        {
            specialty.IsActive = !specialty.IsActive;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int specialtyId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var specialty = await _context.Specialties.FindAsync(specialtyId);
        if (specialty != null)
        {
            _context.Specialties.Remove(specialty);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa chuyên khoa!";
        }
        return RedirectToPage();
    }
}
