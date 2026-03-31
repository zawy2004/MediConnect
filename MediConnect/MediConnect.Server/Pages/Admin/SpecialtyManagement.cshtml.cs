using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MediConnect.Server.Pages.Admin;

[Authorize(Roles = RoleNames.Admin)]
public class SpecialtyManagementModel : PageModel
{
    private readonly IAdminPortalService _adminPortalService;

    public SpecialtyManagementModel(IAdminPortalService adminPortalService)
    {
        _adminPortalService = adminPortalService;
    }

    public List<SpecialtyDepartmentItemDto> Specialties { get; set; } = new();
    public List<DepartmentDto> Departments { get; set; } = new();

    [BindProperty]
    public CreateSpecialtyDto CreateSpecialtyForm { get; set; } = new();

    [BindProperty]
    public UpdateSpecialtyDto EditSpecialtyForm { get; set; } = new();

    [BindProperty]
    public CreateDepartmentDto CreateDepartmentForm { get; set; } = new();

    [BindProperty]
    public UpdateDepartmentDto EditDepartmentForm { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostCreateSpecialtyAsync()
    {
        var created = await _adminPortalService.CreateSpecialtyAsync(CreateSpecialtyForm);
        StatusMessage = created
            ? "Đã tạo chuyên khoa mới."
            : "Không thể tạo chuyên khoa (tên trống hoặc đã tồn tại).";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateSpecialtyAsync()
    {
        var updated = await _adminPortalService.UpdateSpecialtyAsync(EditSpecialtyForm);
        StatusMessage = updated
            ? "Đã cập nhật chuyên khoa."
            : "Không thể cập nhật chuyên khoa.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleSpecialtyStatusAsync(int specialtyId, bool isActive)
    {
        var changed = await _adminPortalService.SetSpecialtyActiveAsync(specialtyId, isActive);
        StatusMessage = changed
            ? (isActive ? "Đã kích hoạt chuyên khoa." : "Đã tạm dừng chuyên khoa.")
            : "Không thể cập nhật trạng thái chuyên khoa.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateDepartmentAsync()
    {
        var created = await _adminPortalService.CreateDepartmentAsync(CreateDepartmentForm);
        StatusMessage = created
            ? "Đã tạo khoa/phòng mới."
            : "Không thể tạo khoa/phòng (tên trống hoặc đã tồn tại).";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateDepartmentAsync()
    {
        var updated = await _adminPortalService.UpdateDepartmentAsync(EditDepartmentForm);
        StatusMessage = updated
            ? "Đã cập nhật khoa/phòng."
            : "Không thể cập nhật khoa/phòng.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleDepartmentStatusAsync(int departmentId, bool isActive)
    {
        var changed = await _adminPortalService.SetDepartmentActiveAsync(departmentId, isActive);
        StatusMessage = changed
            ? (isActive ? "Đã kích hoạt khoa/phòng." : "Đã tạm dừng khoa/phòng.")
            : "Không thể cập nhật trạng thái khoa/phòng.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetExportForecastPdfAsync()
    {
        var data = await _adminPortalService.GetSpecialtyDepartmentAsync();
        var specialties = data.SpecialtyItems.OrderByDescending(x => x.DoctorCount).ToList();
        var departments = data.Departments.OrderBy(x => x.DepartmentName).ToList();

        var totalDoctors = specialties.Sum(x => x.DoctorCount);
        var activeSpecialtyCount = specialties.Count(x => x.IsActive);
        var topSpecialty = specialties.FirstOrDefault();

        Settings.License = LicenseType.Community;

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("MediConnect - Bao cao du bao nhu cau chuyen khoa").FontSize(18).Bold();
                    col.Item().Text($"Ngay tao: {DateTime.Now:dd/MM/yyyy HH:mm}").FontColor(Colors.Grey.Darken2);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text("Tong quan").FontSize(13).Bold();
                    col.Item().Text($"Tong chuyen khoa: {specialties.Count}");
                    col.Item().Text($"Chuyen khoa dang hoat dong: {activeSpecialtyCount}");
                    col.Item().Text($"Tong bac si phan bo: {totalDoctors}");
                    col.Item().Text($"Nhu cau cao nhat: {topSpecialty?.SpecialtyName ?? "N/A"} ({topSpecialty?.DoctorCount ?? 0} bac si)");

                    col.Item().PaddingTop(4).Text("Phan bo chuyen khoa").FontSize(13).Bold();
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellHeader).Text("Chuyen khoa");
                            header.Cell().Element(CellHeader).Text("Mo ta");
                            header.Cell().Element(CellHeader).Text("So bac si");
                            header.Cell().Element(CellHeader).Text("Trang thai");
                        });

                        foreach (var item in specialties)
                        {
                            table.Cell().Element(CellBody).Text(item.SpecialtyName);
                            table.Cell().Element(CellBody).Text(string.IsNullOrWhiteSpace(item.Description) ? "-" : item.Description);
                            table.Cell().Element(CellBody).AlignRight().Text(item.DoctorCount.ToString());
                            table.Cell().Element(CellBody).Text(item.IsActive ? "Hoat dong" : "Tam dung");
                        }
                    });

                    col.Item().PaddingTop(4).Text("Danh sach khoa/phong").FontSize(13).Bold();
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(5);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellHeader).Text("Khoa/phong");
                            header.Cell().Element(CellHeader).Text("Mo ta");
                            header.Cell().Element(CellHeader).Text("Vi tri");
                        });

                        foreach (var item in departments)
                        {
                            table.Cell().Element(CellBody).Text(item.DepartmentName);
                            table.Cell().Element(CellBody).Text(string.IsNullOrWhiteSpace(item.Description) ? "-" : item.Description);
                            table.Cell().Element(CellBody).Text(string.IsNullOrWhiteSpace(item.Location) ? "-" : item.Location);
                        }
                    });
                });

                page.Footer().AlignRight().Text("MediConnect Forecast Export").FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        }).GeneratePdf();

        var fileName = $"forecast-report-{DateTime.Now:yyyyMMdd-HHmmss}.pdf";
        return File(bytes, "application/pdf", fileName);

        static IContainer CellHeader(IContainer container)
            => container.Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten4).Padding(6).DefaultTextStyle(x => x.SemiBold());

        static IContainer CellBody(IContainer container)
            => container.Border(1).BorderColor(Colors.Grey.Lighten1).Padding(6);
    }

    private async Task LoadDataAsync()
    {
        var data = await _adminPortalService.GetSpecialtyDepartmentAsync();
        Specialties = data.SpecialtyItems;
        Departments = data.Departments;
    }
}
