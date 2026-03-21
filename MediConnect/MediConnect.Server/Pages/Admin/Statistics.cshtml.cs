using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Admin;

[Authorize(Roles = RoleNames.Admin)]
public class StatisticsModel : PageModel
{
    private readonly IAdminPortalService _adminPortalService;

    public StatisticsModel(IAdminPortalService adminPortalService)
    {
        _adminPortalService = adminPortalService;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    public AdminStatisticsDto Data { get; set; } = new();

    public async Task OnGetAsync()
    {
        Data = await _adminPortalService.GetStatisticsAsync(FromDate, ToDate);
    }
}
