using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text;

namespace MediConnect.Server.Pages.Admin;

[Authorize(Roles = RoleNames.Admin)]
public class ComplaintManagementModel : PageModel
{
    private readonly IAdminPortalService _adminPortalService;

    public ComplaintManagementModel(IAdminPortalService adminPortalService)
    {
        _adminPortalService = adminPortalService;
    }

    [BindProperty(SupportsGet = true)]
    public int? SelectedComplaintId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty]
    public int ComplaintId { get; set; }

    [BindProperty]
    public string ResolutionNote { get; set; } = string.Empty;

    [BindProperty]
    public string NextStatus { get; set; } = "RESOLVED";

    public AdminComplaintDto Data { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Data = await _adminPortalService.GetComplaintsAsync(SelectedComplaintId);
        Data.Complaints = ApplyFilters(Data.Complaints);
    }

    public async Task<IActionResult> OnPostResolveAsync()
    {
        if (string.IsNullOrWhiteSpace(ResolutionNote))
        {
            StatusMessage = "Vui lòng nhập nội dung phản hồi trước khi gửi.";
            return RedirectToPage(new { SelectedComplaintId = ComplaintId, SearchTerm, StatusFilter });
        }

        var updated = await _adminPortalService.ResolveComplaintAsync(ComplaintId, GetUserId(), ResolutionNote, NextStatus);
        StatusMessage = updated
            ? "Đã cập nhật xử lý khiếu nại."
            : "Không thể cập nhật khiếu nại. Vui lòng thử lại.";

        return RedirectToPage(new { SelectedComplaintId = ComplaintId, SearchTerm, StatusFilter });
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        var data = await _adminPortalService.GetComplaintsAsync(SelectedComplaintId);
        var rows = ApplyFilters(data.Complaints);

        var csv = new StringBuilder();
        csv.AppendLine("ComplaintId,Subject,PatientName,DoctorName,Status,CreatedAt,ResolutionNote");

        foreach (var item in rows)
        {
            csv.AppendLine(string.Join(",",
                item.ComplaintId,
                CsvCell(item.Subject),
                CsvCell(item.PatientName),
                CsvCell(item.DoctorName),
                CsvCell(item.Status),
                item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                CsvCell(item.ResolutionNote)));
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        var fileName = $"complaints-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private List<ComplaintItemDto> ApplyFilters(IEnumerable<ComplaintItemDto> complaints)
    {
        var query = complaints;

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var term = SearchTerm.Trim();
            query = query.Where(c =>
                c.Subject.Contains(term, StringComparison.OrdinalIgnoreCase)
                || c.PatientName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(c.DoctorName) && c.DoctorName.Contains(term, StringComparison.OrdinalIgnoreCase))
                || c.Description.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(StatusFilter) && !string.Equals(StatusFilter, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(c => string.Equals(c.Status, StatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
    }

    private static string CsvCell(string? value)
    {
        var safe = (value ?? string.Empty).Replace("\"", "\"\"");
        return $"\"{safe}\"";
    }
}
