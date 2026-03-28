using System.Security.Claims;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace MediConnect.Server.ViewComponents;

public class PatientHeaderNotificationsViewComponent : ViewComponent
{
    private readonly IPatientPortalService _patientPortalService;

    public PatientHeaderNotificationsViewComponent(IPatientPortalService patientPortalService)
    {
        _patientPortalService = patientPortalService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (User?.Identity?.IsAuthenticated != true || !User.IsInRole(RoleNames.Patient))
            return Content(string.Empty);

        var idClaim = (User as ClaimsPrincipal)?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var patientId))
            return Content(string.Empty);

        var unread = await _patientPortalService.GetUnreadInAppNotificationCountAsync(patientId);
        return View(unread);
    }
}
