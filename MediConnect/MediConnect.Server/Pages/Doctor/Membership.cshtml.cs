using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediConnect.Application.Interfaces;
using MediConnect.Application.DTOs;
using System.Security.Claims;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class MembershipModel : PageModel
{
    private readonly IDoctorPortalService _doctorPortalService;

    public MembershipModel(IDoctorPortalService doctorPortalService)
    {
        _doctorPortalService = doctorPortalService;
    }

    [BindProperty]
    public string PlanCode { get; set; } = "PREMIUM";

    public DoctorMembershipDto Data { get; set; } = new();

    public async Task OnGetAsync()
    {
        Data = await _doctorPortalService.GetMembershipAsync(GetUserId());
        PlanCode = Data.CurrentPlanCode;
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("/Doctor/Payment", new { planCode = PlanCode });
    }

    private int GetUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
