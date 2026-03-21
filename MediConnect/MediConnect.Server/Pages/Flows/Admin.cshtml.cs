using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Flows;

[Authorize(Roles = RoleNames.Admin)]
public class AdminModel : PageModel
{
    public IReadOnlyList<FlowScreen> Screens { get; } =
    [
        new("admin-specialty-config", "Cấu hình chuyên khoa", "Thiết lập chuyên khoa, mô tả và bác sĩ phụ trách."),
        new("doctor-dashboard-9", "Báo cáo & phân tích", "Bảng báo cáo tổng hợp và AI anomaly detection."),
        new("doctor-dashboard-4", "Theo dõi hiệu suất", "Dashboard phân tích hiệu suất toàn hệ thống."),
        new("doctor-dashboard-6", "Phê duyệt lịch hẹn", "Xác nhận hoặc từ chối yêu cầu khám."),
        new("doctor-dashboard-12", "Hồ sơ y tế tổng quan", "Xem hồ sơ bệnh án và tài liệu kết quả."),
    ];

    public sealed record FlowScreen(string Key, string Title, string Description);
}
