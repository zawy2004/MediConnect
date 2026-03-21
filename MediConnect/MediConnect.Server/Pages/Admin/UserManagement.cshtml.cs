using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MediConnect.Server.Pages.Admin;

[Authorize(Roles = RoleNames.Admin)]
public class UserManagementModel : PageModel
{
    private readonly IAdminPortalService _adminPortalService;

    public UserManagementModel(IAdminPortalService adminPortalService)
    {
        _adminPortalService = adminPortalService;
    }

    [BindProperty(SupportsGet = true)]
    public string? RoleFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public List<SystemUserItemDto> Users { get; set; } = new();
    public List<DoctorApprovalItemDto> PendingDoctors { get; set; } = new();

    public async Task OnGetAsync()
    {
        var data = await _adminPortalService.GetUserManagementAsync(SearchTerm, RoleFilter);
        Users = data.Users;
        PendingDoctors = data.PendingDoctors;
    }

    public async Task<IActionResult> OnPostApproveDoctorAsync(int doctorProfileId)
    {
        await _adminPortalService.ApproveDoctorAsync(doctorProfileId, GetUserId());
        return RedirectToPage(new { SearchTerm, RoleFilter });
    }

    public async Task<IActionResult> OnPostRejectDoctorAsync(int doctorProfileId)
    {
        await _adminPortalService.RejectDoctorAsync(doctorProfileId, GetUserId());
        return RedirectToPage(new { SearchTerm, RoleFilter });
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
