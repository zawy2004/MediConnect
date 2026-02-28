using System.ComponentModel.DataAnnotations;
using MediConnect.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly MediconnectContext _context;

    public LoginModel(MediconnectContext context)
    {
        _context = context;
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

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == Email && u.IsActive);

        if (user == null)
        {
            ErrorMessage = "Email hoặc mật khẩu không đúng.";
            return Page();
        }

        // TODO: Verify password hash and create authentication cookie/session
        // For now, redirect to home
        return RedirectToPage("/Index");
    }
}
