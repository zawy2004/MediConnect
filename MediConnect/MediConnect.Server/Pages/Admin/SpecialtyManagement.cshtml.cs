using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

    public async Task OnGetAsync()
    {
        var data = await _adminPortalService.GetSpecialtyDepartmentAsync();
        Specialties = data.SpecialtyItems;
        Departments = data.Departments;
    }
}
