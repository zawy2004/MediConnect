using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _authService.LoginAsync(new LoginDto
        {
            Email = Email,
            Password = Password
        });

        if (!result.Success)
        {
            ErrorMessage = result.ErrorMessage;
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId.ToString()!),
            new(ClaimTypes.Name, result.FullName ?? string.Empty),
            new(ClaimTypes.Email, result.Email ?? string.Empty),
            new(ClaimTypes.Role, result.RoleName ?? string.Empty)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        var targetPage = result.RoleName?.ToUpperInvariant() switch
        {
            RoleNames.Admin => "/Admin/Dashboard",
            RoleNames.Doctor => "/Doctor/Dashboard",
            _ => "/Patient/Dashboard"
        };

        return RedirectToPage(targetPage);
    }
}
