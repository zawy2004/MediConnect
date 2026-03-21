using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Prototype;

[Authorize]
public class ViewModel : PageModel
{
    private readonly IWebHostEnvironment _environment;

    public ViewModel(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public string PrototypeUrl { get; private set; } = string.Empty;
    public string ScreenTitle { get; private set; } = "Prototype";

    public IActionResult OnGet(string key)
    {
        if (!Screens.TryGetValue(key, out var screen))
        {
            return NotFound($"Không tìm thấy màn hình '{key}'.");
        }

        var physicalPath = Path.Combine(
            _environment.WebRootPath,
            "prototypes",
            "stitch_remix_of_search_filter_doctors",
            screen.FolderName,
            "code.html");

        if (!System.IO.File.Exists(physicalPath))
        {
            return NotFound($"Template '{screen.FolderName}/code.html' chưa tồn tại trong wwwroot.");
        }

        ScreenTitle = screen.Title;
        PrototypeUrl = $"/prototypes/stitch_remix_of_search_filter_doctors/{screen.FolderName}/code.html";
        return Page();
    }

    public static IReadOnlyDictionary<string, PrototypeScreen> Screens { get; } =
        new Dictionary<string, PrototypeScreen>(StringComparer.OrdinalIgnoreCase)
        {
            ["unified-login"] = new("mediconnect_unified_login", "Đăng nhập hợp nhất"),
            ["password-reset"] = new("secure_password_reset", "Khôi phục mật khẩu"),
            ["patient-register"] = new("patient_account_registration", "Đăng ký bệnh nhân"),
            ["patient-dashboard"] = new("patient_dashboard", "Dashboard bệnh nhân"),
            ["search-doctors-v1"] = new("search_filter_doctors_1", "Tìm bác sĩ - Danh sách"),
            ["search-doctors-empty"] = new("search_filter_doctors_2", "Tìm bác sĩ - Không có kết quả"),
            ["search-doctors-grid"] = new("search_filter_doctors_3", "Tìm bác sĩ - Lưới lịch"),
            ["search-doctors-list"] = new("search_filter_doctors_4", "Tìm bác sĩ - Danh sách nâng cao"),
            ["doctor-booking"] = new("doctor_profile_booking", "Hồ sơ bác sĩ & đặt lịch"),
            ["appointment-manager"] = new("my_appointment_manager", "Quản lý lịch hẹn cá nhân"),
            ["appointment-confirm"] = new("x_c_nh_n_y_u_c_u_h_n_kh_m", "Xác nhận yêu cầu hẹn khám"),
            ["triage-assistant-v1"] = new("ai_health_triage_assistant_1", "Trợ lý phân tích triệu chứng"),
            ["triage-assistant-v2"] = new("ai_health_triage_assistant_2", "Trợ lý sức khỏe AI - Phiên bản 2"),
            ["triage-assistant-v3"] = new("ai_health_triage_assistant_3", "Trợ lý sức khỏe AI - Phiên bản 3"),
            ["triage-assistant-v4"] = new("ai_health_triage_assistant_4", "Trợ lý sức khỏe AI - Phiên bản 4"),
            ["triage-assistant-v5"] = new("ai_health_triage_assistant_5", "Trợ lý sức khỏe AI - Phiên bản 5"),
            ["doctor-dashboard-1"] = new("doctor_dashboard_overview_1", "Doctor Dashboard 1"),
            ["doctor-dashboard-2"] = new("doctor_dashboard_overview_2", "Doctor Dashboard 2"),
            ["doctor-dashboard-3"] = new("doctor_dashboard_overview_3", "Doctor Dashboard 3"),
            ["doctor-dashboard-4"] = new("doctor_dashboard_overview_4", "Doctor Dashboard 4"),
            ["doctor-dashboard-5"] = new("doctor_dashboard_overview_5", "Doctor Dashboard 5"),
            ["doctor-dashboard-6"] = new("doctor_dashboard_overview_6", "Doctor Dashboard 6"),
            ["doctor-dashboard-7"] = new("doctor_dashboard_overview_7", "Doctor Dashboard 7"),
            ["doctor-dashboard-8"] = new("doctor_dashboard_overview_8", "Doctor Dashboard 8"),
            ["doctor-dashboard-9"] = new("doctor_dashboard_overview_9", "Doctor Dashboard 9"),
            ["doctor-dashboard-10"] = new("doctor_dashboard_overview_10", "Doctor Dashboard 10"),
            ["doctor-dashboard-11"] = new("doctor_dashboard_overview_11", "Doctor Dashboard 11"),
            ["doctor-dashboard-12"] = new("doctor_dashboard_overview_12", "Doctor Dashboard 12"),
            ["doctor-registration"] = new("doctor_professional_registration", "Đăng ký chuyên môn bác sĩ"),
            ["doctor-membership"] = new("l_a_ch_n_g_i_th_nh_vi_n_b_c_s", "Lựa chọn gói thành viên bác sĩ"),
            ["doctor-payment"] = new("thanh_to_n_g_i_th_nh_vi_n_b_c_s", "Thanh toán gói thành viên bác sĩ"),
            ["doctor-waitlist"] = new("qu_n_l_danh_s_ch_ch", "Quản lý danh sách chờ"),
            ["admin-specialty-config"] = new("ch_nh_s_a_h_s_b_c_s", "Cấu hình chuyên khoa"),
        };

    public sealed record PrototypeScreen(string FolderName, string Title);
}
