using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class ConsultationModel : PageModel
{
    private readonly IDoctorPortalService _doctorPortalService;

    public ConsultationModel(IDoctorPortalService doctorPortalService)
    {
        _doctorPortalService = doctorPortalService;
    }

    [BindProperty(SupportsGet = true)]
    public int AppointmentId { get; set; }

    // Bước 1: Khám lâm sàng ban đầu
    [BindProperty]
    public string Symptoms { get; set; } = string.Empty;

    [BindProperty]
    public string BloodPressure { get; set; } = string.Empty;

    [BindProperty]
    public string HeartRate { get; set; } = string.Empty;

    [BindProperty]
    public string Temperature { get; set; } = string.Empty;

    [BindProperty]
    public string Weight { get; set; } = string.Empty;

    [BindProperty]
    public string PhysicalExamination { get; set; } = string.Empty;

    [BindProperty]
    public string PreliminaryDiagnosis { get; set; } = string.Empty;

    // Bước 2: Chỉ định cận lâm sàng
    [BindProperty]
    public bool LabTestBlood { get; set; }

    [BindProperty]
    public bool LabTestUrine { get; set; }

    [BindProperty]
    public bool LabTestXray { get; set; }

    [BindProperty]
    public bool LabTestCT { get; set; }

    [BindProperty]
    public bool LabTestMRI { get; set; }

    [BindProperty]
    public bool LabTestUltrasound { get; set; }

    [BindProperty]
    public string LabTestDetails { get; set; } = string.Empty;

    [BindProperty]
    public string LabTestResults { get; set; } = string.Empty;

    // Bước 3: Chẩn đoán & Kê đơn
    [BindProperty]
    public string Diagnosis { get; set; } = string.Empty;

    [BindProperty]
    public string TreatmentPlan { get; set; } = string.Empty;

    [BindProperty]
    public string Prescription { get; set; } = string.Empty;

    [BindProperty]
    public string Notes { get; set; } = string.Empty;

    [BindProperty]
    public DateTime? FollowUpDate { get; set; }

    public DoctorConsultationDto? Data { get; set; }
    public string? DoctorName { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    // Tính bước hiện tại dựa trên dữ liệu đã nhập
    public int CurrentStep
    {
        get
        {
            if (!string.IsNullOrEmpty(Diagnosis)) return 3;
            if (!string.IsNullOrEmpty(LabTestDetails) || !string.IsNullOrEmpty(LabTestResults) ||
                LabTestBlood || LabTestUrine || LabTestXray || LabTestCT || LabTestMRI || LabTestUltrasound) return 2;
            if (!string.IsNullOrEmpty(Symptoms) || !string.IsNullOrEmpty(PreliminaryDiagnosis)) return 1;
            return 1;
        }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (AppointmentId > 0)
        {
            Data = await _doctorPortalService.GetConsultationAsync(GetUserId(), AppointmentId);
            if (Data != null)
            {
                Symptoms = Data.Symptoms ?? string.Empty;
                Diagnosis = Data.Diagnosis ?? string.Empty;
                TreatmentPlan = Data.TreatmentPlan ?? string.Empty;
                Prescription = Data.Prescription ?? string.Empty;
                Notes = Data.Notes ?? string.Empty;
            }
        }

        DoctorName = User.FindFirstValue(ClaimTypes.Name) ?? "Bác sĩ";
        return Page();
    }

    public async Task<IActionResult> OnPostSaveDraftAsync()
    {
        if (AppointmentId <= 0)
        {
            return RedirectToPage("/Doctor/Dashboard");
        }

        // Gộp tất cả thông tin vào các field hiện có
        var fullSymptoms = BuildFullSymptoms();
        var fullDiagnosis = BuildFullDiagnosis();

        var success = await _doctorPortalService.CompleteConsultationAsync(
            GetUserId(),
            AppointmentId,
            fullSymptoms,
            fullDiagnosis,
            TreatmentPlan,
            Prescription,
            Notes);

        StatusMessage = "Đã lưu nháp thành công.";
        return RedirectToPage(new { AppointmentId });
    }

    public async Task<IActionResult> OnPostCompleteAsync()
    {
        if (AppointmentId <= 0)
        {
            return RedirectToPage("/Doctor/Dashboard");
        }

        // Gộp tất cả thông tin vào các field hiện có
        var fullSymptoms = BuildFullSymptoms();
        var fullDiagnosis = BuildFullDiagnosis();

        var success = await _doctorPortalService.CompleteConsultationAsync(
            GetUserId(),
            AppointmentId,
            fullSymptoms,
            fullDiagnosis,
            TreatmentPlan,
            Prescription,
            Notes);

        if (!success)
        {
            StatusMessage = "Không thể hoàn tất khám. Vui lòng thử lại.";
            return RedirectToPage("/Doctor/Dashboard");
        }

        StatusMessage = "Hoàn tất khám và lưu bệnh án.";
        return RedirectToPage(new { AppointmentId });
    }

    public async Task<IActionResult> OnGetExportPdfAsync(int appointmentId)
    {
        AppointmentId = appointmentId;
        Data = await _doctorPortalService.GetConsultationAsync(GetUserId(), appointmentId);
        
        if (Data == null)
        {
            return NotFound();
        }

        Symptoms = Data.Symptoms ?? string.Empty;
        Diagnosis = Data.Diagnosis ?? string.Empty;
        TreatmentPlan = Data.TreatmentPlan ?? string.Empty;
        Prescription = Data.Prescription ?? string.Empty;
        Notes = Data.Notes ?? string.Empty;
        DoctorName = User.FindFirstValue(ClaimTypes.Name) ?? "Bác sĩ";

        var pdfBytes = GenerateMedicalReportPdf();
        var fileName = $"GiayKham_{Data.PatientName.Replace(" ", "_")}_{Data.AppointmentDate:yyyyMMdd}.pdf";
        
        return File(pdfBytes, "application/pdf", fileName);
    }

    public async Task<IActionResult> OnPostSendToPatientAsync()
    {
        if (AppointmentId <= 0)
        {
            StatusMessage = "Không tìm thấy thông tin lịch khám.";
            return RedirectToPage(new { AppointmentId });
        }

        Data = await _doctorPortalService.GetConsultationAsync(GetUserId(), AppointmentId);
        if (Data == null)
        {
            StatusMessage = "Không tìm thấy thông tin lịch khám.";
            return RedirectToPage(new { AppointmentId });
        }

        Symptoms = Data.Symptoms ?? string.Empty;
        Diagnosis = Data.Diagnosis ?? string.Empty;
        TreatmentPlan = Data.TreatmentPlan ?? string.Empty;
        Prescription = Data.Prescription ?? string.Empty;
        Notes = Data.Notes ?? string.Empty;
        DoctorName = User.FindFirstValue(ClaimTypes.Name) ?? "Bác sĩ";

        // Tạo notification cho bệnh nhân
        var sent = await _doctorPortalService.SendConsultationResultToPatientAsync(
            GetUserId(),
            AppointmentId,
            Data.PatientId);

        if (sent)
        {
            StatusMessage = $"Đã gửi kết quả khám cho bệnh nhân {Data.PatientName} thành công!";
        }
        else
        {
            StatusMessage = "Đã lưu kết quả. Thông báo sẽ được gửi đến bệnh nhân.";
        }

        return RedirectToPage(new { AppointmentId });
    }

    private byte[] GenerateMedicalReportPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("MEDICONNECT").Bold().FontSize(18).FontColor(Colors.Blue.Medium);
                    col.Item().Text("Hệ thống Y tế Thông minh").FontSize(10).FontColor(Colors.Grey.Medium);
                });
                
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text("GIẤY KHÁM BỆNH").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                    col.Item().Text($"Ngày: {DateTime.Now:dd/MM/yyyy}").FontSize(10);
                });
            });

            column.Item().PaddingVertical(10).LineHorizontal(2).LineColor(Colors.Blue.Medium);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Column(column =>
        {
            // Thông tin bệnh nhân
            column.Item().PaddingBottom(15).Element(ComposePatientInfo);
            
            // Kết quả khám
            column.Item().PaddingBottom(15).Element(ComposeExaminationResults);
            
            // Chẩn đoán
            column.Item().PaddingBottom(15).Element(ComposeDiagnosis);
            
            // Đơn thuốc
            if (!string.IsNullOrWhiteSpace(Prescription))
            {
                column.Item().PaddingBottom(15).Element(ComposePrescription);
            }
            
            // Lời khuyên
            if (!string.IsNullOrWhiteSpace(TreatmentPlan))
            {
                column.Item().PaddingBottom(15).Element(ComposeAdvice);
            }
        });
    }

    private void ComposePatientInfo(IContainer container)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(column =>
        {
            column.Item().Text("THÔNG TIN BỆNH NHÂN").Bold().FontSize(12).FontColor(Colors.Blue.Darken1);
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Họ và tên: {Data?.PatientName ?? "N/A"}").FontSize(11);
                    col.Item().Text($"Giới tính: {Data?.PatientGender ?? "N/A"}").FontSize(11);
                    col.Item().Text($"Tuổi: {Data?.PatientAge ?? 0}").FontSize(11);
                });
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Nhóm máu: {Data?.BloodType ?? "N/A"}").FontSize(11);
                    col.Item().Text($"Ngày khám: {Data?.AppointmentDate:dd/MM/yyyy}").FontSize(11);
                    col.Item().Text($"Bác sĩ: {DoctorName}").FontSize(11);
                });
            });
        });
    }

    private void ComposeExaminationResults(IContainer container)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(column =>
        {
            column.Item().Text("KẾT QUẢ KHÁM LÂM SÀNG").Bold().FontSize(12).FontColor(Colors.Blue.Darken1);
            column.Item().PaddingTop(10).Text(text =>
            {
                text.Span("Triệu chứng & Lý do khám: ").Bold();
                text.Span(Data?.Reason ?? "Không có thông tin");
            });
            
            if (!string.IsNullOrWhiteSpace(Symptoms))
            {
                column.Item().PaddingTop(8).Text(text =>
                {
                    text.Span("Chi tiết khám: ").Bold();
                });
                column.Item().PaddingLeft(10).Text(Symptoms).FontSize(10);
            }
        });
    }

    private void ComposeDiagnosis(IContainer container)
    {
        container.Background(Colors.Blue.Lighten5).Border(1).BorderColor(Colors.Blue.Lighten3).Padding(15).Column(column =>
        {
            column.Item().Text("CHẨN ĐOÁN").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
            column.Item().PaddingTop(10).Text(Diagnosis).FontSize(11);
        });
    }

    private void ComposePrescription(IContainer container)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(column =>
        {
            column.Item().Text("ĐƠN THUỐC").Bold().FontSize(12).FontColor(Colors.Green.Darken1);
            column.Item().PaddingTop(10).Text(Prescription).FontSize(10);
        });
    }

    private void ComposeAdvice(IContainer container)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Column(column =>
        {
            column.Item().Text("LỜI KHUYÊN & CHẾ ĐỘ DINH DƯỠNG").Bold().FontSize(12).FontColor(Colors.Orange.Darken1);
            column.Item().PaddingTop(10).Text(TreatmentPlan).FontSize(10);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Ghi chú:").Bold().FontSize(9);
                    col.Item().Text("- Giấy khám này có giá trị trong 7 ngày").FontSize(8).FontColor(Colors.Grey.Medium);
                    col.Item().Text("- Vui lòng mang theo giấy này khi tái khám").FontSize(8).FontColor(Colors.Grey.Medium);
                });
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().AlignCenter().Text($"Bác sĩ khám bệnh").FontSize(10);
                    col.Item().PaddingTop(40).AlignCenter().Text(DoctorName ?? "").Bold().FontSize(11);
                });
            });
        });
    }

    private string BuildFullSymptoms()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(Symptoms))
            parts.Add($"[Triệu chứng] {Symptoms}");

        var vitals = new List<string>();
        if (!string.IsNullOrWhiteSpace(BloodPressure)) vitals.Add($"Huyết áp: {BloodPressure}");
        if (!string.IsNullOrWhiteSpace(HeartRate)) vitals.Add($"Nhịp tim: {HeartRate} bpm");
        if (!string.IsNullOrWhiteSpace(Temperature)) vitals.Add($"Nhiệt độ: {Temperature}°C");
        if (!string.IsNullOrWhiteSpace(Weight)) vitals.Add($"Cân nặng: {Weight} kg");
        if (vitals.Count > 0)
            parts.Add($"[Sinh hiệu] {string.Join(", ", vitals)}");

        if (!string.IsNullOrWhiteSpace(PhysicalExamination))
            parts.Add($"[Khám tổng quát] {PhysicalExamination}");

        if (!string.IsNullOrWhiteSpace(PreliminaryDiagnosis))
            parts.Add($"[Chẩn đoán sơ bộ] {PreliminaryDiagnosis}");

        return string.Join("\n\n", parts);
    }

    private string BuildFullDiagnosis()
    {
        var parts = new List<string>();

        // Thêm thông tin xét nghiệm
        var labTests = new List<string>();
        if (LabTestBlood) labTests.Add("Xét nghiệm máu");
        if (LabTestUrine) labTests.Add("Xét nghiệm nước tiểu");
        if (LabTestXray) labTests.Add("Chụp X-quang");
        if (LabTestCT) labTests.Add("Chụp CT");
        if (LabTestMRI) labTests.Add("Chụp MRI");
        if (LabTestUltrasound) labTests.Add("Siêu âm");

        if (labTests.Count > 0)
            parts.Add($"[Chỉ định xét nghiệm] {string.Join(", ", labTests)}");

        if (!string.IsNullOrWhiteSpace(LabTestDetails))
            parts.Add($"[Chi tiết xét nghiệm] {LabTestDetails}");

        if (!string.IsNullOrWhiteSpace(LabTestResults))
            parts.Add($"[Kết quả xét nghiệm] {LabTestResults}");

        if (!string.IsNullOrWhiteSpace(Diagnosis))
            parts.Add($"[Chẩn đoán cuối cùng] {Diagnosis}");

        if (FollowUpDate.HasValue)
            parts.Add($"[Hẹn tái khám] {FollowUpDate.Value:dd/MM/yyyy}");

        return string.Join("\n\n", parts);
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
