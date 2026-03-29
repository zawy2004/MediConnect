using System.Linq;
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
    private readonly IPaymentGatewayService _paymentGatewayService;

    public PaymentModel(
        IDoctorPortalService doctorPortalService,
        IPaymentGatewayService paymentGatewayService)
    {
        _doctorPortalService = doctorPortalService;
        _paymentGatewayService = paymentGatewayService;
    }

    [BindProperty(SupportsGet = true)]
    public string PlanCode { get; set; } = "PREMIUM";

    [BindProperty]
    public string PaymentMethod { get; set; } = "VNPAY";

    public DoctorPaymentSummaryDto Summary { get; set; } = new();
    [BindProperty(SupportsGet = true)]
    public string SuccessMessage { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        Summary = await _doctorPortalService.GetPaymentSummaryAsync(GetUserId(), PlanCode);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Summary = await _doctorPortalService.GetPaymentSummaryAsync(GetUserId(), PlanCode);

        if (PaymentMethod.Equals("VNPAY", StringComparison.OrdinalIgnoreCase) ||
            PaymentMethod.Equals("MOMO", StringComparison.OrdinalIgnoreCase))
        {
            var paymentRequest = new CreatePaymentRequestDto
            {
                AppointmentId = 0,
                PatientId = GetUserId(),
                Amount = Summary.Amount,
                Currency = "VND",
                PaymentMethod = PaymentMethod.ToUpperInvariant(),
                OrderInfo = $"DOCTOR_MEMBERSHIP|doctorId={GetUserId()}|planCode={PlanCode}",
                ClientIpAddress = GetClientIpAddress(),
                ReturnUrl = Url.Page("/Doctor/PaymentReturn", null, null, Request.Scheme)
            };

            var paymentResult = await _paymentGatewayService.CreatePaymentUrlAsync(paymentRequest);
            if (!paymentResult.Success || string.IsNullOrEmpty(paymentResult.PaymentUrl))
            {
                ErrorMessage = paymentResult.ErrorMessage ?? "Không thể tạo link thanh toán. Vui lòng thử lại.";
                return Page();
            }

            return Redirect(paymentResult.PaymentUrl);
        }

        var success = await _doctorPortalService.SubmitMembershipPaymentAsync(GetUserId(), PlanCode, PaymentMethod);
        Summary = await _doctorPortalService.GetPaymentSummaryAsync(GetUserId(), PlanCode);
        SuccessMessage = success
            ? "Thanh toán thành công. Gói thành viên đã được kích hoạt."
            : "Thanh toán thất bại. Vui lòng thử lại.";
        return Page();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string GetClientIpAddress()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            ip = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        }

        return ip ?? "127.0.0.1";
    }
}
