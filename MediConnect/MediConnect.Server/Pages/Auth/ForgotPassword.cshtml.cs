using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using MediConnect.Server.Data;
using MediConnect.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MediConnect.Server.Pages.Auth;

public class ForgotPasswordModel : PageModel
{
    private readonly MediconnectContext _context;

    public ForgotPasswordModel(MediconnectContext context)
    {
        _context = context;
    }

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string? Token { get; set; }

    [BindProperty]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải ít nhất 8 ký tự")]
    public string? NewPassword { get; set; }

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public bool ShowStep2 { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostSendCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Vui lòng nhập email.";
            return Page();
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
        if (user == null)
        {
            // Don't reveal if email exists
            SuccessMessage = "Nếu email tồn tại, mã xác thực đã được gửi.";
            ShowStep2 = true;
            return Page();
        }

        // Generate 6-digit token
        var tokenValue = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

        var resetToken = new PasswordResetToken
        {
            UserId = user.UserId,
            Token = tokenValue,
            ExpiresAt = DateTime.Now.AddMinutes(15),
            IsUsed = false,
            CreatedAt = DateTime.Now
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        // TODO: Send email with tokenValue
        // For now, show step 2 directly
        SuccessMessage = "Mã xác thực đã được gửi đến email của bạn.";
        ShowStep2 = true;
        return Page();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(Token) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Vui lòng điền đầy đủ thông tin.";
            ShowStep2 = true;
            return Page();
        }

        var resetToken = await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == Token
                && t.User.Email == Email
                && !t.IsUsed
                && t.ExpiresAt > DateTime.Now);

        if (resetToken == null)
        {
            ErrorMessage = "Mã xác thực không hợp lệ hoặc đã hết hạn.";
            ShowStep2 = true;
            return Page();
        }

        resetToken.IsUsed = true;
        resetToken.User.PasswordHash = NewPassword; // TODO: Hash with BCrypt
        resetToken.User.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return RedirectToPage("/Auth/Login", new { message = "password_reset" });
    }
}
