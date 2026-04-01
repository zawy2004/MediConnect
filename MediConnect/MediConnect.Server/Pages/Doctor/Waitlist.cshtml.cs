using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Security.Claims;

namespace MediConnect.Server.Pages.Doctor;

[Authorize(Roles = RoleNames.Doctor)]
public class WaitlistModel : PageModel
{
    private readonly IDoctorPortalService _doctorPortalService;

    public WaitlistModel(IDoctorPortalService doctorPortalService)
    {
        _doctorPortalService = doctorPortalService;
    }

    public DoctorWaitlistDto Data { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string CalendarMode { get; set; } = "week";

    public DateOnly CalendarReference { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public List<DateOnly> CalendarWeekDates { get; set; } = new();
    public List<DateOnly> CalendarMonthDates { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Data = await _doctorPortalService.GetWaitlistAsync(GetUserId());
        BuildCalendarRanges();
    }

    private void BuildCalendarRanges()
    {
        var today = CalendarReference;
        var startOfWeek = today.DayOfWeek == DayOfWeek.Sunday ? today.AddDays(-6) : today.AddDays(1 - (int)today.DayOfWeek);
        CalendarWeekDates = Enumerable.Range(0, 7).Select(offset => startOfWeek.AddDays(offset)).ToList();

        var firstOfMonth = new DateOnly(today.Year, today.Month, 1);
        CalendarMonthDates = Enumerable.Range(0, DateTime.DaysInMonth(today.Year, today.Month)).Select(offset => firstOfMonth.AddDays(offset)).ToList();
    }

    public async Task<IActionResult> OnPostNotifyNextAsync()
    {
        var ok = await _doctorPortalService.NotifyNextWaitlistAsync(GetUserId());
        StatusMessage = ok ? "Đã gửi thông báo cho bệnh nhân kế tiếp." : "Không có bệnh nhân chờ để thông báo.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveUpAsync(int waitlistId)
    {
        await _doctorPortalService.MoveWaitlistUpAsync(waitlistId, GetUserId());
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMoveDownAsync(int waitlistId)
    {
        await _doctorPortalService.MoveWaitlistDownAsync(waitlistId, GetUserId());
        return RedirectToPage();
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
