using System.ComponentModel.DataAnnotations;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Patient;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly IAuthService _authService;

    public RegisterModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty, Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [BindProperty, Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [BindProperty, Phone]
    public string? PhoneNumber { get; set; }

    [BindProperty, Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [BindProperty, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.RegisterAsync(new RegisterDto
        {
            FullName = FullName,
            Email = Email,
            PhoneNumber = PhoneNumber,
            Password = Password
        });

        if (!result.Success)
        {
            ErrorMessage = result.ErrorMessage;
            return Page();
        }

        SuccessMessage = "Đăng ký thành công. Vui lòng đăng nhập để tiếp tục.";
        return Page();
    }
}
