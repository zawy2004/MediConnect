using System.ComponentModel.DataAnnotations;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Patient;

[Authorize(Roles = RoleNames.Patient)]
public class DoctorProfileModel : PageModel
{
    private readonly IPatientPortalService _patientPortalService;
    public DoctorProfileModel(IPatientPortalService patientPortalService)
    {
        _patientPortalService = patientPortalService;
    }

    [BindProperty(SupportsGet = true)]
    public int DoctorProfileId { get; set; }

    [BindProperty, Required]
    public int SlotId { get; set; }

    [BindProperty]
    public string? Reason { get; set; }

    public PatientDoctorProfileDto? Data { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (DoctorProfileId <= 0)
        {
            return RedirectToPage("/Patient/SearchDoctors");
        }

        Data = await _patientPortalService.GetDoctorProfileAsync(DoctorProfileId);
        if (Data == null)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostBookAsync()
    {
        Data = await _patientPortalService.GetDoctorProfileAsync(DoctorProfileId);
        if (Data?.Doctor == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var slot = Data.NextSlots.FirstOrDefault(s => s.SlotId == SlotId);
        if (slot == null)
        {
            ErrorMessage = "Khung giờ không hợp lệ hoặc đã hết chỗ.";
            return Page();
        }

        // Chuyển sang màn xác nhận thanh toán để bệnh nhân chọn phương thức trước.
        return RedirectToPage(
            "/Patient/PaymentConfirm",
            new
            {
                doctorProfileId = DoctorProfileId,
                slotId = SlotId,
                reason = Reason
            });
    }
}
