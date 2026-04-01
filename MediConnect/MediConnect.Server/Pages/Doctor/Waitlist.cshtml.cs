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

    [BindProperty(SupportsGet = true)]
    public string FilterCategory { get; set; } = "all";

    [BindProperty(SupportsGet = true)]
    public DateTime? FilterPreferredDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FilterPreferredTime { get; set; }

    public DateOnly CalendarReference { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public List<DateOnly> CalendarWeekDates { get; set; } = new();
    public List<DateOnly> CalendarMonthDates { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Data = await _doctorPortalService.GetWaitlistAsync(GetUserId());

        if (!string.Equals(FilterCategory, "all", StringComparison.OrdinalIgnoreCase))
        {
            Data.Items = FilterCategory.ToLowerInvariant() switch
            {
                "waiting" => Data.Items.Where(i => i.Status == "WAITING").ToList(),
                "notified" => Data.Items.Where(i => i.Status == "NOTIFIED").ToList(),
                _ => Data.Items
            };
        }

        if (FilterPreferredDate.HasValue)
        {
            var dateFilter = DateOnly.FromDateTime(FilterPreferredDate.Value.Date);
            Data.Items = Data.Items.Where(i => i.PreferredDate == dateFilter).ToList();
        }

        if (!string.IsNullOrWhiteSpace(FilterPreferredTime) && TimeOnly.TryParse(FilterPreferredTime, out var timeFilter))
        {
            Data.Items = Data.Items.Where(i => i.PreferredTime == timeFilter).ToList();
        }

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
        return RedirectToPage(new { FilterCategory, FilterPreferredDate, FilterPreferredTime, CalendarMode });
    }

    public async Task<IActionResult> OnPostConfirmAsync(int waitlistId)
    {
        var ok = await _doctorPortalService.ConfirmWaitlistAsync(waitlistId, GetUserId());
        StatusMessage = ok ? "Bệnh nhân đã được xác nhận trong danh sách chờ." : "Không thể xác nhận bệnh nhân này.";
        return RedirectToPage(new { FilterCategory, FilterPreferredDate, FilterPreferredTime, CalendarMode });
    }

    public async Task<IActionResult> OnPostScheduleAsync(int waitlistId)
    {
        var ok = await _doctorPortalService.ScheduleWaitlistAsync(waitlistId, GetUserId());
        StatusMessage = ok ? "Bệnh nhân đã được chuyển thành lịch khám." : "Không thể chuyển bệnh nhân này thành lịch khám.";
        return RedirectToPage(new { FilterCategory, FilterPreferredDate, FilterPreferredTime, CalendarMode });
    }

    public async Task<IActionResult> OnPostResetWaitPerformanceAsync()
    {
        var ok = await _doctorPortalService.ResetWaitlistPerformanceAsync(GetUserId());
        StatusMessage = ok ? "Đã reset lại hiệu suất chờ và thời gian chờ trung bình." : "Không có dữ liệu chờ để reset.";
        return RedirectToPage(new { FilterCategory, FilterPreferredDate, FilterPreferredTime, CalendarMode });
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
