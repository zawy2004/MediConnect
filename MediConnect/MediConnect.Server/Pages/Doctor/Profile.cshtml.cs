using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Doctor;

public class ProfileModel : PageModel
{
    private readonly MediconnectContext _context;

    public ProfileModel(MediconnectContext context)
    {
        _context = context;
    }

    public DoctorProfile? DoctorProfile { get; set; }
    public List<Specialty> DoctorSpecialties { get; set; } = new();

    [BindProperty] public string FullName { get; set; } = "";
    [BindProperty] public string? PhoneNumber { get; set; }
    [BindProperty] public string? Address { get; set; }
    [BindProperty] public string? Bio { get; set; }
    [BindProperty] public string? Education { get; set; }
    [BindProperty] public string? Location { get; set; }
    [BindProperty] public decimal ConsultationFee { get; set; }
    [BindProperty] public string? InsuranceAccepted { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        DoctorProfile = await _context.DoctorProfiles
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (DoctorProfile == null) return RedirectToPage("/Auth/Login");

        FullName = DoctorProfile.User.FullName;
        PhoneNumber = DoctorProfile.User.PhoneNumber;
        Address = DoctorProfile.User.Address;
        Bio = DoctorProfile.Bio;
        Education = DoctorProfile.Education;
        Location = DoctorProfile.Location;
        ConsultationFee = DoctorProfile.ConsultationFee;
        InsuranceAccepted = DoctorProfile.InsuranceAccepted;

        DoctorSpecialties = await _context.DoctorSpecialties
            .Where(ds => ds.UserId == userId)
            .Include(ds => ds.Specialty)
            .Select(ds => ds.Specialty)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToPage("/Auth/Login");

        var user = await _context.Users.FindAsync(userId);
        var profile = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
        if (user == null || profile == null) return RedirectToPage("/Auth/Login");

        user.FullName = FullName;
        user.PhoneNumber = PhoneNumber;
        user.Address = Address;
        user.UpdatedAt = DateTime.Now;

        profile.Bio = Bio;
        profile.Education = Education;
        profile.Location = Location;
        profile.ConsultationFee = ConsultationFee;
        profile.InsuranceAccepted = InsuranceAccepted;

        await _context.SaveChangesAsync();
        HttpContext.Session.SetString("UserName", FullName);

        TempData["Success"] = "Cập nhật hồ sơ thành công!";
        return RedirectToPage();
    }
}
