using System.ComponentModel.DataAnnotations;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;

    public RegisterModel(IAuthService authService)
    {
        _authService = authService;
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

        var result = await _authService.RegisterAsync(new RegisterDto
        {
            FullName = FullName,
            Email = Email,
            Password = Password,
            PhoneNumber = PhoneNumber,
            Gender = Gender,
            DateOfBirth = DateOfBirth
        });

        if (!result.Success)
        {
            ErrorMessage = result.ErrorMessage;
            return Page();
        }

        SuccessMessage = "Đăng ký thành công! Vui lòng đăng nhập.";
        return Page();
    }
}
