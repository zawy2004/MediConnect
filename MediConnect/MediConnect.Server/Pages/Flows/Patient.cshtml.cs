using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Flows;

[Authorize(Roles = RoleNames.Patient)]
public class PatientModel : PageModel
{
    public IReadOnlyList<FlowScreen> Screens { get; } =
    [
        new("unified-login", "Đăng nhập hợp nhất", "Màn hình đăng nhập cho bệnh nhân/bác sĩ."),
        new("password-reset", "Quên mật khẩu", "Luồng khôi phục mật khẩu an toàn."),
        new("patient-register", "Đăng ký tài khoản", "Form đăng ký bệnh nhân theo từng bước."),
        new("patient-dashboard", "Dashboard bệnh nhân", "Tổng quan sức khỏe, lịch hẹn, gợi ý AI."),
        new("search-doctors-v1", "Tìm bác sĩ - Danh sách", "Tìm và lọc bác sĩ theo tiêu chí."),
        new("search-doctors-empty", "Tìm bác sĩ - Không kết quả", "Trạng thái không có bác sĩ phù hợp."),
        new("search-doctors-grid", "Tìm bác sĩ - Lưới thời gian", "Xếp hạng AI và lịch trống dạng lưới."),
        new("search-doctors-list", "Tìm bác sĩ - Danh sách nâng cao", "Danh sách bác sĩ kèm lịch trống 7 ngày."),
        new("doctor-booking", "Hồ sơ bác sĩ & đặt lịch", "Chi tiết bác sĩ, đánh giá và khung giờ."),
        new("patient-payment-confirm", "Xác nhận & thanh toán", "Bước xác nhận thông tin và thanh toán lịch hẹn."),
        new("booking-success", "Đặt lịch thành công", "Màn hình xác nhận sau thanh toán thành công."),
        new("patient-profile", "Hồ sơ bệnh nhân", "Xem và cập nhật thông tin tài khoản bệnh nhân."),
        new("appointment-manager", "Quản lý lịch hẹn", "Lịch hẹn sắp tới/lịch sử của bệnh nhân."),
        new("triage-assistant-v1", "AI Triage Assistant", "Trợ lý phân tích triệu chứng và gợi ý khoa khám."),
        new("triage-assistant-v2", "AI Triage Assistant 2", "Biến thể giao diện triage assistant."),
    ];

    public sealed record FlowScreen(string Key, string Title, string Description);
}
