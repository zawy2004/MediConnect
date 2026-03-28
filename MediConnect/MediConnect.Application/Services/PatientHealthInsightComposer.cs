using MediConnect.Domain.Constants;
using MediConnect.Domain.Entities;

namespace MediConnect.Application.Services;

/// <summary>
/// Tạo 3 gợi ý sức khỏe ngắn, ngẫu nhiên có kiểm soát (seed theo bệnh nhân + thời điểm),
/// phù hợp khung giờ / mùa / lịch hẹn — không gọi API ngoài.
/// </summary>
public static class PatientHealthInsightComposer
{
    public static IReadOnlyList<string> Compose(int patientId, string patientName, DateTime localNow, IReadOnlyList<Appointment> appointments)
    {
        var firstName = string.IsNullOrWhiteSpace(patientName)
            ? "bạn"
            : patientName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? "bạn";

        var today = DateOnly.FromDateTime(localNow);
        var hour = localNow.Hour;
        var dow = localNow.DayOfWeek;
        var month = localNow.Month;

        var upcoming = appointments
            .Where(a => a.AppointmentDate >= today
                        && a.Status != AppointmentStatus.CancelledByPatient
                        && a.Status != AppointmentStatus.CancelledByDoctor)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToList();

        var upcomingCount = upcoming.Count;
        var completed = appointments.Count(a => a.Status == AppointmentStatus.Completed);
        var cancelled = appointments.Count(a =>
            a.Status == AppointmentStatus.CancelledByPatient || a.Status == AppointmentStatus.CancelledByDoctor);

        var nextWithin48h = upcoming.FirstOrDefault(a =>
        {
            var dt = a.AppointmentDate.ToDateTime(a.StartTime);
            return dt >= localNow && dt <= localNow.AddHours(48);
        });

        var pool = new List<string>();

        // Khung giờ
        if (hour is >= 5 and < 11)
        {
            pool.Add($"Buổi sáng tốt lành, {firstName} — uống một cốc nước ấm trước khi ra khỏi nhà giúp cơ thể tỉnh táo và hỗ trợ tiêu hóa nhẹ nhàng.");
            pool.Add("Sáng nay nếu phải chờ khám, mang thêm một lớp mỏng: phòng lạnh dễ làm co mạch và khô cổ họng.");
            pool.Add("Khung giờ sáng thường ít xếp hàng hơn; nếu được, ưu tiên lịch trước 10 giờ để giảm thời gian chờ.");
        }
        else if (hour is >= 11 and < 14)
        {
            pool.Add("Trưa nên nghỉ ngắn 10–15 phút, tránh nhịn ăn quá lâu nếu bạn đã dùng thuốc cần bữa ăn.");
            pool.Add("Ăn trưa vừa đủ, giảm đồ cay nóng nếu hôm nay có lịch khám sau đó để dễ theo dõi triệu chứng.");
        }
        else if (hour is >= 14 and < 18)
        {
            pool.Add("Chiều là lúc cơ thể dễ mệt — nếu lái xe đi khám, nghỉ 5 phút, duỗi vai cổ trước khi vào bệnh viện.");
            pool.Add("Nhắc nhở nhẹ: kiểm tra giấy tờ và giờ hẹn; đến sớm 10–15 phút giúp tâm lý thoải mái hơn.");
        }
        else if (hour is >= 18 and < 22)
        {
            pool.Add("Tối là thời điểm nên kiểm tra lại lịch ngày mai: đặt báo thức và chuẩn bị hồ sơ để sáng mai không vội.");
            pool.Add("Hạn chế cà phê muộn nếu sáng mai bạn có xét nghiệm hoặc đo huyết áp — dễ ngủ hơn và kết quả ổn định hơn.");
        }
        else
        {
            pool.Add("Khuya rồi — nếu không cấp cứu, ưu tiên nghỉ ngơi; đặt lịch khám vào khung giờ hành chính sẽ thuận tiện hơn.");
            pool.Add("Trước khi ngủ, ghi nhớ triệu chứng trong ngày (nếu có) để trình bày rõ với bác sĩ ở lần sau.");
        }

        // Cuối tuần / ngày thường
        if (dow is DayOfWeek.Saturday or DayOfWeek.Sunday)
            pool.Add("Cuối tuần phòng khám có thể đông hơn; nếu được, chọn ca sáng hoặc đặt lịch online trước để chủ động.");
        else
            pool.Add("Trong tuần, các khung giờ đầu giờ chiều đôi khi vẫn còn lịch trống — kiểm tra app khi bạn linh hoạt thời gian.");

        // Mùa (góc nhìn VN)
        if (month is >= 5 and <= 10)
            pool.Add("Mùa mưa ẩm: giữ chân khô, quần áo thoáng; nếu ho kéo dài hơn một tuần, nên đặt lịch tai mũi họng hoặc nội.");
        if (month is 12 or 1 or 2)
            pool.Add("Tiết trời lạnh: người có huyết áp hay thay đổi theo thời tiết nên tránh ra gió đột ngột sau khi ở phòng ấm.");
        if (month is >= 3 and <= 4)
            pool.Add("Giao mùa: dị ứng phấn hoa và viêm mũi dị ứng tăng — rửa mũi saline nhẹ và theo dõi nếu khó thở.");

        // Ngữ cảnh lịch hẹn
        if (upcomingCount == 0)
            pool.Add("Bạn chưa có lịch sắp tới — khi có dấu hiệu bất thường, dùng đánh giá triệu chứng rồi chọn chuyên khoa sẽ đỡ lãng phí thời gian.");
        else
            pool.Add("Giữ nhịp tái khám đúng hẹn giúp bác sĩ kết nối được các kết quả cũ – mới và điều chỉnh điều trị an toàn.");

        if (nextWithin48h != null)
            pool.Add(
                $"Lịch gần nhất của bạn là {nextWithin48h.AppointmentDate:dd/MM} lúc {nextWithin48h.StartTime:HH:mm} — mang theo đơn thuốc và mô tả triệu chứng cho bác sĩ (nhớ nói rõ thời điểm nặng nhất trong ngày, {firstName} nhé).");

        if (completed >= 3)
            pool.Add("Bạn đã tích lũy vài lần khám — lưu ý mang theo kết quả cũ (nếu đổi cơ sở) để tránh xét nghiệm trùng không cần thiết.");

        if (cancelled >= 2)
            pool.Add("Nếu gần đây bạn hủy lịch vài lần: thử chọn khung giờ cách xa giờ cao điểm hoặc cuối tuần để hạn chế xung đột lịch.");

        // Kiến thức chung / gợi ý sức khỏe
        pool.Add("Trước khi đặt lịch, mô tả triệu chứng theo thứ tự: khi nào bắt đầu, mức đau 1–10, yếu tố làm tăng/giảm — bác sĩ dễ khai thác hơn.");
        pool.Add("Khi chờ trong phòng khám, thở chậm 4–6 nhịp/phút giúp giảm căng thẳng và huyết áp tạm thời không “nhảy” mạnh.");
        pool.Add("Ưu tiên nước lọc, hạn chế nước ngọt trước khi lấy máu hoặc siêu âm bụng nếu được yêu cầu nhịn — tuân thủ hướng dẫn trước xét nghiệm.");
        pool.Add("Nếu dùng thuốc định kỳ, chụp ảnh vỏ hộp hoặc mang theo để tránh chia sẻ sai tên hoạt chất khi khám.");
        pool.Add("Đặt báo nhắc uống thuốc trên điện thoại; nhịp ổn định quan trọng không kém so với lựa chọn loại thuốc.");
        pool.Add("Với các triệu chứng cấp như đau ngực dữ, liệt nửa người, khó thở đột ngột — ưu tiên cấp cứu 115, không chỉ đặt lịch khám.");

        // Seed xáo trộn ổn định theo ~4 giờ + ngày + bệnh nhân
        var slot = hour / 4;
        var seed = HashCode.Combine(patientId, today.Year, today.DayNumber, slot);
        var rnd = new Random(seed);
        var list = pool.ToList();
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rnd.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list.Take(Math.Min(3, list.Count)).ToList();
    }
}
