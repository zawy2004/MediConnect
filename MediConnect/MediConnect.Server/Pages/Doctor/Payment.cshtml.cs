using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediConnect.Application.Interfaces;
using MediConnect.Application.DTOs;
using System.Security.Claims;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class PaymentModel : PageModel
{
    private readonly IDoctorPortalService _doctorPortalService;

    public PaymentModel(IDoctorPortalService doctorPortalService)
    {
        _doctorPortalService = doctorPortalService;
    }

    [BindProperty(SupportsGet = true)]
    public string PlanCode { get; set; } = "PREMIUM";

    [BindProperty]
    public string PaymentMethod { get; set; } = "VNPAY";

    public DoctorPaymentSummaryDto Summary { get; set; } = new();
    public string SuccessMessage { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        Summary = await _doctorPortalService.GetPaymentSummaryAsync(GetUserId(), PlanCode);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var success = await _doctorPortalService.SubmitMembershipPaymentAsync(GetUserId(), PlanCode, PaymentMethod);
        Summary = await _doctorPortalService.GetPaymentSummaryAsync(GetUserId(), PlanCode);
        SuccessMessage = success
            ? "Thanh toán thành công. Gói thành viên đã được kích hoạt."
            : "Thanh toán thất bại. Vui lòng thử lại.";
        return Page();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
