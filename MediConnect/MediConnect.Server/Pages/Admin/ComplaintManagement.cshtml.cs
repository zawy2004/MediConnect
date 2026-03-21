using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

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
    }

    public async Task<IActionResult> OnPostResolveAsync()
    {
        await _adminPortalService.ResolveComplaintAsync(ComplaintId, GetUserId(), ResolutionNote, NextStatus);
        StatusMessage = "Đã cập nhật xử lý khiếu nại.";
        return RedirectToPage(new { SelectedComplaintId = ComplaintId });
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
