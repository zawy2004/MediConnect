using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Flows;

[Authorize(Roles = RoleNames.Doctor)]
public class DoctorModel : PageModel
{
    public IReadOnlyList<FlowScreen> Screens { get; } =
    [
        new("doctor-registration", "Đăng ký chuyên môn bác sĩ", "Luồng đăng ký và xác thực chứng chỉ."),
        new("doctor-dashboard", "Dashboard bác sĩ", "Tổng quan bệnh nhân và lịch khám."),
        new("doctor-confirm-request", "Xác nhận yêu cầu hẹn khám", "Duyệt yêu cầu đặt lịch từ bệnh nhân."),
        new("doctor-patient-record", "Xem hồ sơ bệnh nhân", "Thông tin lâm sàng và lịch sử khám."),
        new("doctor-performance", "Phân tích hiệu suất", "Thống kê chất lượng khám và AI insight."),
        new("doctor-specialty-profile", "Hồ sơ chuyên môn", "Hồ sơ chuyên khoa hiển thị cho bệnh nhân."),
        new("doctor-profile-update", "Cập nhật hồ sơ chuyên môn", "Chỉnh sửa thông tin chuyên môn bác sĩ."),
        new("doctor-membership", "Lựa chọn gói thành viên", "Trang chọn gói dịch vụ bác sĩ."),
        new("doctor-payment", "Thanh toán gói thành viên", "Trang thanh toán membership."),
        new("doctor-waitlist", "Quản lý danh sách chờ", "Thứ tự bệnh nhân dự phòng và thông báo."),
    ];

    public sealed record FlowScreen(string Key, string Title, string Description);
}
