using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace MediConnect.Server.Pages.Admin;

[Authorize(Roles = RoleNames.Admin)]
public class UserManagementModel : PageModel
{
    private readonly IAdminPortalService _adminPortalService;

    public UserManagementModel(IAdminPortalService adminPortalService)
    {
        _adminPortalService = adminPortalService;
    }

    [BindProperty(SupportsGet = true)]
    public string? RoleFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SelectedUserId { get; set; }

    [BindProperty]
    public EditUserDto EditForm { get; set; } = new();

    [BindProperty]
    public CreateAdminUserDto CreateUserForm { get; set; } = new();

    [BindProperty]
    public List<int> SelectedUserIds { get; set; } = new();

    public List<SystemUserItemDto> Users { get; set; } = new();
    public List<DoctorApprovalItemDto> PendingDoctors { get; set; } = new();
    public UserDetailDto? SelectedUserDetail { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadUsersAsync();
        
        if (SelectedUserId.HasValue)
        {
            SelectedUserDetail = await _adminPortalService.GetUserDetailAsync(SelectedUserId.Value);
        }
    }

    public async Task<IActionResult> OnPostApproveDoctorAsync(int doctorProfileId)
    {
        await _adminPortalService.ApproveDoctorAsync(doctorProfileId, GetUserId());
        return RedirectToPage(new { SearchTerm, RoleFilter });
    }

    public async Task<IActionResult> OnPostRejectDoctorAsync(int doctorProfileId)
    {
        await _adminPortalService.RejectDoctorAsync(doctorProfileId, GetUserId());
        return RedirectToPage(new { SearchTerm, RoleFilter });
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(int userId, bool isActive)
    {
        var adminUserId = GetUserId();

        // Avoid locking out current admin session by self-disable.
        if (userId == adminUserId && !isActive)
        {
            StatusMessage = "Không thể tự vô hiệu hóa tài khoản đang đăng nhập.";
            return RedirectToPage(new { SearchTerm, RoleFilter, SelectedUserId = (int?)null });
        }

        var success = await _adminPortalService.ToggleUserStatusAsync(userId, isActive, adminUserId);
        StatusMessage = success
            ? "Trạng thái người dùng đã được cập nhật."
            : "Không thể cập nhật trạng thái người dùng.";
        return RedirectToPage(new { SearchTerm, RoleFilter, SelectedUserId = (int?)null });
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(int userId)
    {
        var success = await _adminPortalService.ResetUserPasswordAsync(userId, GetUserId());
        StatusMessage = success
            ? "Mật khẩu đã được đặt lại. Email xác nhận đã được gửi cho người dùng."
            : "Không thể đặt lại mật khẩu cho người dùng này.";
        return RedirectToPage(new { SearchTerm, RoleFilter, SelectedUserId });
    }

    public async Task<IActionResult> OnPostUpdateUserAsync()
    {
        if (!ModelState.IsValid)
        {
            StatusMessage = "Dữ liệu không hợp lệ.";
            return Page();
        }

        var success = await _adminPortalService.UpdateUserAsync(EditForm, GetUserId());
        StatusMessage = success
            ? "Thông tin người dùng đã được cập nhật thành công."
            : "Không thể cập nhật thông tin người dùng.";
        return RedirectToPage(new { SearchTerm, RoleFilter, SelectedUserId = EditForm.UserId });
    }

    public async Task<IActionResult> OnPostCreateUserAsync()
    {
        var success = await _adminPortalService.CreateUserAsync(CreateUserForm, GetUserId());
        StatusMessage = success
            ? "Tài khoản mới đã được tạo thành công."
            : "Không thể tạo tài khoản (kiểm tra email, mật khẩu hoặc vai trò).";
        return RedirectToPage(new { SearchTerm, RoleFilter });
    }

    public async Task<IActionResult> OnGetExportUsersAsync()
    {
        var bytes = await _adminPortalService.ExportUsersExcelAsync(SelectedUserIds.Count > 0 ? SelectedUserIds : null);
        var fileName = $"users-export-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> OnPostExportUsersAsync()
    {
        var bytes = await _adminPortalService.ExportUsersExcelAsync(SelectedUserIds.Count > 0 ? SelectedUserIds : null);
        var fileName = $"users-export-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> OnGetUserDetailAsync(int userId)
    {
        var userDetail = await _adminPortalService.GetUserDetailAsync(userId);
        if (userDetail == null)
        {
            return NotFound();
        }

        return new JsonResult(new
        {
            fullName = userDetail.FullName,
            email = userDetail.Email,
            phone = userDetail.PhoneNumber,
            gender = userDetail.Gender,
            dob = userDetail.DateOfBirth?.ToString("dd/MM/yyyy"),
            address = userDetail.Address,
            roleName = userDetail.RoleName,
            isActive = userDetail.IsActive,
            createdAt = userDetail.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
            activities = userDetail.RecentActivities.Select(a => new
            {
                action = a.Action,
                description = a.Description ?? "Không có mô tả",
                severity = a.Severity,
                createdAt = a.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            }).ToList()
        });
    }

    private async Task LoadUsersAsync()
    {
        var data = await _adminPortalService.GetUserManagementAsync(SearchTerm, RoleFilter);
        Users = data.Users;
        PendingDoctors = data.PendingDoctors;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
