using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Admin;

[Authorize(Roles = RoleNames.Admin)]
public class ZaloNotificationModel : PageModel
{
    private readonly IAdminPortalService _adminPortalService;

    public ZaloNotificationModel(IAdminPortalService adminPortalService)
    {
        _adminPortalService = adminPortalService;
    }

    public AdminZaloNotificationDto Data { get; set; } = new();

    public async Task OnGetAsync()
    {
        Data = await _adminPortalService.GetZaloNotificationAsync();
    }
}
