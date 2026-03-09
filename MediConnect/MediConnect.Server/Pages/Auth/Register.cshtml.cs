using System.ComponentModel.DataAnnotations;
using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly MediconnectContext _context;

    public RegisterModel(MediconnectContext context)
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
    public string? Gender { get; set; }

    [BindProperty]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [BindProperty]
    public string? Address { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải ít nhất 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == Email))
        {
            ErrorMessage = "Email này đã được sử dụng.";
            return Page();
        }

        var patientRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "PATIENT");
        if (patientRole == null)
        {
            ErrorMessage = "Lỗi hệ thống: Không tìm thấy vai trò bệnh nhân.";
            return Page();
        }

        var user = new User
        {
            RoleId = patientRole.RoleId,
            FullName = FullName,
            Email = Email,
            PasswordHash = Password, // TODO: Hash password with BCrypt
            PhoneNumber = PhoneNumber,
            Gender = Gender,
            DateOfBirth = DateOfBirth.HasValue ? DateOnly.FromDateTime(DateOfBirth.Value) : null,
            Address = Address,
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        SuccessMessage = "Đăng ký thành công! Vui lòng đăng nhập.";
        return Page();
    }
}
