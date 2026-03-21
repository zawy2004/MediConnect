using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Flows;

[Authorize(Roles = RoleNames.Doctor)]
public class DoctorModel : PageModel
{
    public IReadOnlyList<FlowScreen> Screens { get; } =
    [
        new("/Doctor/ProfessionalRegistration", "Đăng ký chuyên môn bác sĩ", "Luồng đăng ký và xác thực chứng chỉ."),
        new("/Doctor/Dashboard", "Dashboard bác sĩ", "Tổng quan bệnh nhân và lịch khám."),
        new("/Doctor/ConfirmAppointmentRequest", "Xác nhận yêu cầu hẹn khám", "Duyệt yêu cầu đặt lịch từ bệnh nhân."),
        new("/Doctor/PatientRecord", "Xem hồ sơ bệnh nhân", "Thông tin lâm sàng và lịch sử khám."),
        new("/Doctor/PerformanceAnalysis", "Phân tích hiệu suất", "Thống kê chất lượng khám và AI insight."),
        new("/Doctor/SpecialtyProfile", "Hồ sơ chuyên môn", "Hồ sơ chuyên khoa hiển thị cho bệnh nhân."),
        new("/Doctor/Profile", "Cập nhật hồ sơ chuyên môn", "Chỉnh sửa thông tin chuyên môn bác sĩ."),
        new("/Doctor/Membership", "Lựa chọn gói thành viên", "Trang chọn gói dịch vụ bác sĩ."),
        new("/Doctor/Payment", "Thanh toán gói thành viên", "Trang thanh toán membership."),
        new("/Doctor/Waitlist", "Quản lý danh sách chờ", "Thứ tự bệnh nhân dự phòng và thông báo."),
    ];

    public sealed record FlowScreen(string Key, string Title, string Description);
}
