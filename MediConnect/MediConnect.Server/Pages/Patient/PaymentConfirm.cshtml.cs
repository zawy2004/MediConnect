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

    public PaymentConfirmModel(IPatientPortalService patientPortalService)
    {
        _patientPortalService = patientPortalService;
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

        var payment = await _patientPortalService.CompletePaymentAsync(GetUserId(), AppointmentId, PaymentMethod);
        if (!payment.Success)
        {
            ErrorMessage = payment.ErrorMessage;
            return Page();
        }

        return RedirectToPage("/Patient/BookingSuccess", new { appointmentId = AppointmentId });
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
