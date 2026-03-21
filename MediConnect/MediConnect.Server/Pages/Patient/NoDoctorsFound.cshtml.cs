using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class NoDoctorsFoundModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SpecialtyId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DepartmentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MinRating { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Insurance { get; set; }

    public string FilterSummary =>
        $"Từ khóa: {SearchTerm ?? "(trống)"}, Chuyên khoa: {(SpecialtyId?.ToString() ?? "Tất cả")}, Khoa: {(DepartmentId?.ToString() ?? "Tất cả")}, Rating >= {(MinRating?.ToString("0.0") ?? "không")}";
}
