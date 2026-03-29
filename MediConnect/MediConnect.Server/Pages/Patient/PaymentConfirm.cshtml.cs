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

    [BindProperty(SupportsGet = true)]
    public int DoctorProfileId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int SlotId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Reason { get; set; }

    [BindProperty, Required]
    public string PaymentMethod { get; set; } = "VNPAY";

    public PatientPaymentConfirmDto? Summary { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (AppointmentId > 0)
        {
            Summary = await _patientPortalService.GetPaymentConfirmAsync(GetUserId(), AppointmentId);
        }
        else if (DoctorProfileId > 0 && SlotId > 0)
        {
            Summary = await _patientPortalService.GetPaymentConfirmByDoctorSlotAsync(GetUserId(), DoctorProfileId, SlotId);
        }
        else
        {
            return RedirectToPage("/Patient/Appointments");
        }

        if (Summary == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (AppointmentId > 0)
        {
            Summary = await _patientPortalService.GetPaymentConfirmAsync(GetUserId(), AppointmentId);
        }
        else if (DoctorProfileId > 0 && SlotId > 0)
        {
            Summary = await _patientPortalService.GetPaymentConfirmByDoctorSlotAsync(GetUserId(), DoctorProfileId, SlotId);
        }

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
            var paymentAppointmentId = AppointmentId > 0 ? AppointmentId : (int?)null;
            var orderInfo = $"Thanh toan lich kham #{AppointmentId} - BS {Summary.DoctorName}";

            if (DoctorProfileId > 0 && SlotId > 0)
            {
                var doctorProfile = await _patientPortalService.GetDoctorProfileAsync(DoctorProfileId);
                if (doctorProfile?.Doctor == null)
                {
                    ErrorMessage = "Không thể tải thông tin bác sĩ để thanh toán.";
                    return Page();
                }

                orderInfo =
                    $"doctorUserId={doctorProfile.Doctor.UserId};slotId={SlotId};appointmentDate={Summary.AppointmentDate:yyyy-MM-dd};reason={Uri.EscapeDataString(Reason ?? string.Empty)}";
            }

            var paymentRequest = new CreatePaymentRequestDto
            {
                AppointmentId = paymentAppointmentId,
                PatientId = GetUserId(),
                Amount = Summary.Amount,
                Currency = "VND",
                PaymentMethod = PaymentMethod.ToUpperInvariant(),
                OrderInfo = orderInfo,
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

        if (DoctorProfileId > 0 && SlotId > 0)
        {
            ErrorMessage = "Lịch chưa được tạo. Vui lòng chọn thanh toán online (VNPAY hoặc MoMo).";
            return Page();
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
