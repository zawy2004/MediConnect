using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class PaymentConfirmModel : PageModel
{
    private readonly IPatientPortalService _patientPortalService;
    private readonly IPaymentGatewayService _paymentGatewayService;

    public PaymentConfirmModel(
        IPatientPortalService patientPortalService,
        IPaymentGatewayService paymentGatewayService)
    {
        _patientPortalService = patientPortalService;
        _paymentGatewayService = paymentGatewayService;
    }

    [BindProperty(SupportsGet = true)]
    public int AppointmentId { get; set; }

    [BindProperty, Required]
    public string PaymentMethod { get; set; } = "VNPAY";

    public PatientPaymentConfirmDto? Summary { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (AppointmentId <= 0)
        {
            return RedirectToPage("/Patient/Appointments");
        }

        Summary = await _patientPortalService.GetPaymentConfirmAsync(GetUserId(), AppointmentId);
        if (Summary == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Summary = await _patientPortalService.GetPaymentConfirmAsync(GetUserId(), AppointmentId);
        if (Summary == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Handle online payment methods (VNPay, MoMo)
        if (PaymentMethod.Equals("VNPAY", StringComparison.OrdinalIgnoreCase) ||
            PaymentMethod.Equals("MOMO", StringComparison.OrdinalIgnoreCase))
        {
            var paymentRequest = new CreatePaymentRequestDto
            {
                AppointmentId = AppointmentId,
                PatientId = GetUserId(),
                Amount = Summary.Amount,
                Currency = "VND",
                PaymentMethod = PaymentMethod.ToUpperInvariant(),
                OrderInfo = $"Thanh toan lich kham #{AppointmentId} - BS {Summary.DoctorName}",
                ClientIpAddress = GetClientIpAddress()
            };

            var paymentResult = await _paymentGatewayService.CreatePaymentUrlAsync(paymentRequest);

            if (!paymentResult.Success || string.IsNullOrEmpty(paymentResult.PaymentUrl))
            {
                ErrorMessage = paymentResult.ErrorMessage ?? "Không thể tạo link thanh toán. Vui lòng thử lại.";
                return Page();
            }

            // Redirect to payment gateway
            return Redirect(paymentResult.PaymentUrl);
        }

        // Handle on-site payment (ONSITE, CARD at clinic)
        var payment = await _patientPortalService.CompletePaymentAsync(GetUserId(), AppointmentId, PaymentMethod);
        if (!payment.Success)
        {
            ErrorMessage = payment.ErrorMessage;
            return Page();
        }

        return RedirectToPage("/Patient/BookingSuccess", new { appointmentId = AppointmentId });
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
