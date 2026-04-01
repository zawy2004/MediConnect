using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediConnect.Domain.Constants;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class SettingsModel : PageModel
{
    [BindProperty]
    public string ThemeMode { get; set; } = "light";

    [BindProperty]
    public string FontSize { get; set; } = "normal";

    [BindProperty]
    public bool CompactLayout { get; set; }

    [BindProperty]
    public bool ShowAvatars { get; set; } = true;

    [TempData]
    public string? StatusMessage { get; set; }

    public void OnGet()
    {
        ThemeMode = Request.Cookies["ui_theme"] ?? "light";
        FontSize = Request.Cookies["ui_font_size"] ?? "normal";
        CompactLayout = Request.Cookies["ui_compact"] == "true";
        ShowAvatars = Request.Cookies["ui_show_avatars"] != "false";
    }

    public IActionResult OnPost()
    {
        Response.Cookies.Append("ui_theme", ThemeMode, new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1) });
        Response.Cookies.Append("ui_font_size", FontSize, new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1) });
        Response.Cookies.Append("ui_compact", CompactLayout.ToString().ToLowerInvariant(), new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1) });
        Response.Cookies.Append("ui_show_avatars", ShowAvatars.ToString().ToLowerInvariant(), new CookieOptions { Expires = DateTimeOffset.Now.AddYears(1) });

        StatusMessage = "Cài đặt giao diện đã được lưu.";
        return RedirectToPage();
    }
}
