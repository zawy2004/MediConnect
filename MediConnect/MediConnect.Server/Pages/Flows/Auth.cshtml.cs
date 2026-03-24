using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Flows;

public class AuthModel : PageModel
{
    public IReadOnlyList<FlowScreen> Screens { get; } =
    [
        new("unified-login", "Đăng nhập hợp nhất", "Màn hình đăng nhập cho bệnh nhân/bác sĩ/admin."),
        new("password-reset", "Khôi phục mật khẩu", "Luồng khôi phục mật khẩu an toàn."),
        new("patient-register", "Đăng ký tài khoản", "Màn hình đăng ký tài khoản mới."),
    ];

    public sealed record FlowScreen(string Key, string Title, string Description);
}