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
public class ProfileModel : PageModel
{
    private readonly IPatientPortalService _patientPortalService;

    public ProfileModel(IPatientPortalService patientPortalService)
    {
        _patientPortalService = patientPortalService;
    }

    public PatientProfilePortalDto Data { get; set; } = new();

    [BindProperty, Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [BindProperty, Phone]
    public string? PhoneNumber { get; set; }

    [BindProperty]
    public string? Gender { get; set; }

    [BindProperty]
    public DateTime? DateOfBirth { get; set; }

    [BindProperty]
    public string? Address { get; set; }

    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var data = await _patientPortalService.GetProfileAsync(GetUserId());
        if (data == null)
        {
            return NotFound();
        }

        Data = data;
        FillForm(data);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var current = await _patientPortalService.GetProfileAsync(GetUserId());
        if (current == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            Data = current;
            return Page();
        }

        var ok = await _patientPortalService.UpdateProfileAsync(
            GetUserId(),
            FullName,
            PhoneNumber,
            Gender,
            DateOfBirth.HasValue ? DateOnly.FromDateTime(DateOfBirth.Value) : null,
            Address);

        Data = (await _patientPortalService.GetProfileAsync(GetUserId())) ?? current;
        FillForm(Data);
        StatusMessage = ok ? "Cập nhật hồ sơ thành công." : "Không thể cập nhật hồ sơ.";
        return Page();
    }

    private void FillForm(PatientProfilePortalDto data)
    {
        FullName = data.FullName;
        PhoneNumber = data.PhoneNumber;
        Gender = data.Gender;
        DateOfBirth = data.DateOfBirth?.ToDateTime(TimeOnly.MinValue);
        Address = data.Address;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
