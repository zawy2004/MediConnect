using System.ComponentModel.DataAnnotations;
using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Auth;

public class DoctorRegisterModel : PageModel
{
    private readonly MediconnectContext _context;

    public DoctorRegisterModel(MediconnectContext context)
    {
        _context = context;
    }

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? PhoneNumber { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải ít nhất 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập số giấy phép")]
    public string LicenseNumber { get; set; } = string.Empty;

    [BindProperty]
    public string? Education { get; set; }

    [BindProperty]
    public int? SpecialtyId { get; set; }

    [BindProperty]
    public int YearsOfExperience { get; set; }

    [BindProperty]
    public decimal ConsultationFee { get; set; }

    [BindProperty]
    public int? DepartmentId { get; set; }

    [BindProperty]
    public string? Bio { get; set; }

    public string? ErrorMessage { get; set; }

    public List<Specialty> Specialties { get; set; } = new();
    public List<Department> Departments { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadLookups();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadLookups();

        if (!ModelState.IsValid) return Page();

        if (await _context.Users.AnyAsync(u => u.Email == Email))
        {
            ErrorMessage = "Email này đã được sử dụng.";
            return Page();
        }

        var doctorRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "DOCTOR");
        if (doctorRole == null)
        {
            ErrorMessage = "Lỗi hệ thống: Không tìm thấy vai trò bác sĩ.";
            return Page();
        }

        var user = new User
        {
            RoleId = doctorRole.RoleId,
            FullName = FullName,
            Email = Email,
            PasswordHash = Password, // TODO: Hash password with BCrypt
            PhoneNumber = PhoneNumber,
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var profile = new DoctorProfile
        {
            UserId = user.UserId,
            DepartmentId = DepartmentId,
            LicenseNumber = LicenseNumber,
            YearsOfExperience = YearsOfExperience,
            Education = Education,
            Bio = Bio,
            ConsultationFee = ConsultationFee,
            ApprovalStatus = "PENDING",
            AverageRating = 0,
            TotalReviews = 0,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.DoctorProfiles.Add(profile);

        if (SpecialtyId.HasValue)
        {
            _context.DoctorSpecialties.Add(new DoctorSpecialty
            {
                UserId = user.UserId,
                SpecialtyId = SpecialtyId.Value,
                IsPrimary = true
            });
        }

        await _context.SaveChangesAsync();

        return RedirectToPage("/Auth/Login", new { message = "doctor_registered" });
    }

    private async Task LoadLookups()
    {
        Specialties = await _context.Specialties.Where(s => s.IsActive).OrderBy(s => s.SpecialtyName).ToListAsync();
        Departments = await _context.Departments.Where(d => d.IsActive).OrderBy(d => d.DepartmentName).ToListAsync();
    }
}
