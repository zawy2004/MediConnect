using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Flows;

[Authorize(Roles = RoleNames.Admin)]
public class AdminModel : PageModel
{
    public IReadOnlyList<FlowScreen> Screens { get; } =
    [
        new("admin-dashboard", "Dashboard quản trị", "Tổng quan số liệu vận hành hệ thống."),
        new("admin-user-management", "Quản lý người dùng", "Duyệt và quản trị tài khoản người dùng."),
        new("admin-specialty-management", "Quản lý chuyên khoa", "Danh sách và phân công chuyên khoa/khoa."),
        new("admin-specialty-config", "Cấu hình chuyên khoa", "Thiết lập chuyên khoa, mô tả và bác sĩ phụ trách."),
        new("admin-statistics", "Thống kê", "Báo cáo KPI và tăng trưởng theo thời gian."),
        new("admin-system-monitoring", "Giám sát hệ thống", "Theo dõi logs, tài nguyên và trạng thái dịch vụ."),
        new("admin-complaint-management", "Khiếu nại & hỗ trợ", "Quản lý ticket và phản hồi hỗ trợ."),
        new("admin-mail-notification", "Thông báo Email", "Cấu hình và theo dõi thông báo Email."),
    ];

    public sealed record FlowScreen(string Key, string Title, string Description);
}
