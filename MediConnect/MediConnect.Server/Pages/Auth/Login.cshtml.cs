using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
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

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

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

        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage(targetPage);
    }

    public IActionResult OnPostGoogle(string? returnUrl = null)
    {
        var redirectUrl = Url.Page("/Auth/Login", "GoogleResponse", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> OnGetGoogleResponseAsync(string? returnUrl = null, string? remoteError = null)
    {
        if (!string.IsNullOrWhiteSpace(remoteError))
        {
            ErrorMessage = $"Đăng nhập Google thất bại: {remoteError}";
            return Page();
        }

        var email = User.FindFirstValue(ClaimTypes.Email);
        var fullName = User.FindFirstValue(ClaimTypes.Name) ?? "Google User";

        if (string.IsNullOrWhiteSpace(email))
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ErrorMessage = "Không lấy được email từ Google.";
            return Page();
        }

        var result = await _authService.LoginOrRegisterGoogleAsync(email, fullName);
        if (!result.Success)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ErrorMessage = result.ErrorMessage;
            return Page();
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId.ToString()!),
            new(ClaimTypes.Name, result.FullName ?? string.Empty),
            new(ClaimTypes.Email, result.Email ?? string.Empty),
            new(ClaimTypes.Role, result.RoleName ?? RoleNames.Patient)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        var targetPage = result.RoleName?.ToUpperInvariant() switch
        {
            RoleNames.Admin => "/Admin/Dashboard",
            RoleNames.Doctor => "/Doctor/Dashboard",
            _ => "/Patient/Dashboard"
        };

        return RedirectToPage(targetPage);
    }
}
