using System.Security.Claims;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class PaymentHistoryModel : PageModel
{
    private readonly IPatientPortalService _patientPortalService;

    public PaymentHistoryModel(IPatientPortalService patientPortalService)
    {
        _patientPortalService = patientPortalService;
    }

    public List<PatientPaymentHistoryItemDto> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        var patientId = GetUserId();
        Items = await _patientPortalService.GetPaidAppointmentPaymentHistoryAsync(patientId, 50);
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

