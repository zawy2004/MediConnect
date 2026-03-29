using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using MediConnect.Domain.Entities;
using System.Net;
using BCrypt.Net;
using OfficeOpenXml;

namespace MediConnect.Application.Services;

public class AdminPortalService : IAdminPortalService
{
    private readonly IUserRepository _userRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly ISpecialtyRepository _specialtyRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDoctorSpecialtyRepository _doctorSpecialtyRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly ISystemLogRepository _systemLogRepository;
    private readonly IComplaintRepository _complaintRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IEmailMessagingService _emailMessagingService;
    private readonly IUnitOfWork _unitOfWork;

    public AdminPortalService(
        IUserRepository userRepository,
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        ISpecialtyRepository specialtyRepository,
        IDepartmentRepository departmentRepository,
        IDoctorSpecialtyRepository doctorSpecialtyRepository,
        IPaymentRepository paymentRepository,
        IReviewRepository reviewRepository,
        ISystemLogRepository systemLogRepository,
        IComplaintRepository complaintRepository,
        INotificationRepository notificationRepository,
        IEmailMessagingService emailMessagingService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _specialtyRepository = specialtyRepository;
        _departmentRepository = departmentRepository;
        _doctorSpecialtyRepository = doctorSpecialtyRepository;
        _paymentRepository = paymentRepository;
        _reviewRepository = reviewRepository;
        _systemLogRepository = systemLogRepository;
        _complaintRepository = complaintRepository;
        _notificationRepository = notificationRepository;
        _emailMessagingService = emailMessagingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AdminOverviewDto> GetOverviewAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var recentAppointments = await _appointmentRepository.GetByDateRangeAsync(today.AddDays(-6), today);
        var todayAppointments = recentAppointments.Where(a => a.AppointmentDate == today).ToList();

        var specialties = await _specialtyRepository.GetActiveAsync();
        var specialtyLoads = specialties
            .Select(s => new SpecialtyLoadDto
            {
                SpecialtyName = s.SpecialtyName,
                Count = recentAppointments.Count(a => a.SpecialtyId == s.SpecialtyId)
            })
            .OrderByDescending(s => s.Count)
            .Take(5)
            .ToList();

        var trend = Enumerable.Range(0, 7)
            .Select(offset => today.AddDays(-6 + offset))
            .Select(d => new DailyCountDto
            {
                Date = d,
                Count = recentAppointments.Count(a => a.AppointmentDate == d)
            })
            .ToList();

        return new AdminOverviewDto
        {
            TotalUsers = await _userRepository.CountAllActiveAsync(),
            TotalAppointmentsToday = todayAppointments.Count,
            ProcessingAppointmentsToday = todayAppointments.Count(a => a.Status == AppointmentStatus.Pending),
            UptimePercent = 99.9m,
            ErrorRatePercent = 0.01m,
            SpecialtyLoads = specialtyLoads,
            TrendCounts = trend
        };
    }

    public async Task<AdminUserManagementDto> GetUserManagementAsync(string? searchTerm, string? roleName)
    {
        var users = await _userRepository.SearchUsersAsync(searchTerm, roleName);
        var pendingDoctors = await _doctorRepository.GetPendingApprovalAsync(50);

        return new AdminUserManagementDto
        {
            Users = users.Select(u => new SystemUserItemDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                RoleName = u.Role.RoleName,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToList(),
            PendingDoctors = pendingDoctors.Select(d => new DoctorApprovalItemDto
            {
                DoctorProfileId = d.DoctorProfileId,
                UserId = d.UserId,
                FullName = d.User.FullName,
                Email = d.User.Email,
                DepartmentName = d.Department?.DepartmentName,
                YearsOfExperience = d.YearsOfExperience,
                CreatedAt = d.CreatedAt
            }).ToList()
        };
    }

    public async Task<bool> ApproveDoctorAsync(int doctorProfileId, int adminUserId)
    {
        var profile = await _doctorRepository.GetDoctorDetailAsync(doctorProfileId);
        if (profile == null) return false;

        profile.ApprovalStatus = DoctorApprovalStatus.Approved;
        profile.ApprovedBy = adminUserId;
        profile.ApprovedAt = DateTime.Now;
        profile.UpdatedAt = DateTime.Now;
        await _doctorRepository.UpdateAsync(profile);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectDoctorAsync(int doctorProfileId, int adminUserId)
    {
        var profile = await _doctorRepository.GetDoctorDetailAsync(doctorProfileId);
        if (profile == null) return false;

        profile.ApprovalStatus = DoctorApprovalStatus.Rejected;
        profile.ApprovedBy = adminUserId;
        profile.ApprovedAt = DateTime.Now;
        profile.UpdatedAt = DateTime.Now;
        await _doctorRepository.UpdateAsync(profile);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<AdminSpecialtyDepartmentDto> GetSpecialtyDepartmentAsync()
    {
        var specialties = await _specialtyRepository.GetActiveAsync();
        var departments = await _departmentRepository.GetActiveAsync();

        var specialtyItems = new List<SpecialtyDepartmentItemDto>();
        foreach (var specialty in specialties)
        {
            var mappings = await _doctorSpecialtyRepository.GetBySpecialtyIdAsync(specialty.SpecialtyId);
            specialtyItems.Add(new SpecialtyDepartmentItemDto
            {
                SpecialtyId = specialty.SpecialtyId,
                SpecialtyName = specialty.SpecialtyName,
                Description = specialty.Description,
                IsActive = specialty.IsActive,
                DoctorCount = mappings.Select(m => m.UserId).Distinct().Count()
            });
        }

        return new AdminSpecialtyDepartmentDto
        {
            SpecialtyItems = specialtyItems.OrderByDescending(s => s.DoctorCount).ToList(),
            Departments = departments.Select(d => new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                Location = d.Location,
                IsActive = d.IsActive
            }).ToList()
        };
    }

    public async Task<AdminSpecialtyConfigDto?> GetSpecialtyConfigAsync(int specialtyId)
    {
        var specialty = await _specialtyRepository.GetByIdAsync(specialtyId);
        if (specialty == null) return null;

        var mappings = await _doctorSpecialtyRepository.GetBySpecialtyIdAsync(specialtyId);
        var doctors = await _doctorRepository.GetApprovedDoctorsAsync(null, null, null);

        return new AdminSpecialtyConfigDto
        {
            SpecialtyId = specialty.SpecialtyId,
            SpecialtyName = specialty.SpecialtyName,
            Description = specialty.Description,
            IconUrl = specialty.IconUrl,
            CandidateDoctors = doctors.Select(d => new DoctorListDto
            {
                DoctorProfileId = d.DoctorProfileId,
                UserId = d.UserId,
                FullName = d.User.FullName,
                DepartmentName = d.Department?.DepartmentName,
                YearsOfExperience = d.YearsOfExperience,
                ConsultationFee = d.ConsultationFee,
                AverageRating = d.AverageRating,
                TotalReviews = d.TotalReviews
            }).ToList(),
            SelectedDoctorUserIds = mappings.Select(m => m.UserId).Distinct().ToList()
        };
    }

    public async Task<bool> SaveSpecialtyConfigAsync(SaveSpecialtyConfigDto dto)
    {
        var specialty = await _specialtyRepository.GetByIdAsync(dto.SpecialtyId);
        if (specialty == null) return false;

        specialty.SpecialtyName = dto.SpecialtyName;
        specialty.Description = dto.Description;
        specialty.IconUrl = dto.IconUrl;
        specialty.UpdatedAt = DateTime.Now;
        await _specialtyRepository.UpdateAsync(specialty);

        await _doctorSpecialtyRepository.RemoveBySpecialtyIdAsync(dto.SpecialtyId);
        var mappings = dto.DoctorUserIds
            .Distinct()
            .Select((userId, index) => new DoctorSpecialty
            {
                UserId = userId,
                SpecialtyId = dto.SpecialtyId,
                IsPrimary = index == 0
            })
            .ToList();

        if (mappings.Count > 0)
        {
            await _doctorSpecialtyRepository.AddRangeAsync(mappings);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<AdminStatisticsDto> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate ?? DateTime.Today.AddDays(-30);
        var to = toDate ?? DateTime.Today;

        var appointments = await _appointmentRepository.GetByDateRangeAsync(DateOnly.FromDateTime(from), DateOnly.FromDateTime(to));
        var paidRevenue = await _paymentRepository.SumPaidAmountAsync(from, to.AddDays(1).AddTicks(-1));
        var reviews = await _reviewRepository.GetByDateRangeAsync(from, to.AddDays(1).AddTicks(-1));
        var complaints = await _complaintRepository.GetAllAsync();

        var completionRate = appointments.Count == 0
            ? 0m
            : Math.Round(appointments.Count(a => a.Status == AppointmentStatus.Completed) * 100m / appointments.Count, 2);

        var csat = reviews.Count == 0
            ? 0m
            : Math.Round((decimal)reviews.Average(r => r.Rating), 2);

        var trend = appointments
            .GroupBy(a => a.AppointmentDate)
            .OrderBy(g => g.Key)
            .Select(g => new DailyCountDto { Date = g.Key, Count = g.Count() })
            .ToList();

        var keywordCandidates = complaints
            .SelectMany(c => (c.Description ?? string.Empty)
                .ToLowerInvariant()
                .Split([' ', ',', '.', ';', ':', '-', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries))
            .Where(w => w.Length >= 4)
            .GroupBy(w => w)
            .OrderByDescending(g => g.Count())
            .Take(8)
            .Select(g => new KeywordWeightDto { Keyword = g.Key, Weight = g.Count() })
            .ToList();

        return new AdminStatisticsDto
        {
            TotalVisits = appointments.Count,
            Revenue = paidRevenue,
            CompletionRatePercent = completionRate,
            Csat = csat,
            AppointmentTrend = trend,
            FeedbackKeywords = keywordCandidates
        };
    }

    public async Task<AdminMonitoringDto> GetMonitoringAsync()
    {
        var logs = await _systemLogRepository.GetRecentAsync(20);

        return new AdminMonitoringDto
        {
            ErrorCount = await _systemLogRepository.CountBySeverityAsync("ERROR"),
            WarningCount = await _systemLogRepository.CountBySeverityAsync("WARNING"),
            InfoCount = await _systemLogRepository.CountBySeverityAsync("INFO"),
            Logs = logs.Select(l => new SystemLogItemDto
            {
                CreatedAt = l.CreatedAt,
                Severity = l.Severity,
                Action = l.Action,
                Description = l.Description,
                Username = l.User?.Email ?? "System"
            }).ToList(),
            BackupProgressPercent = 74,
            PredictedLoadMessage = "Dự báo tải cao vào 09:00 ngày mai. Khuyến nghị mở rộng tài nguyên trước 08:30."
        };
    }

    public async Task<AdminComplaintDto> GetComplaintsAsync(int? selectedComplaintId)
    {
        var complaints = await _complaintRepository.GetAllAsync();
        var logs = await _systemLogRepository.GetRecentAsync(1000);

        var enriched = complaints
            .Select(c => new { Complaint = c, Meta = BuildComplaintMeta(c, logs) })
            .ToList();

        var selected = selectedComplaintId.HasValue
            ? enriched.FirstOrDefault(c => c.Complaint.ComplaintId == selectedComplaintId)
            : enriched.FirstOrDefault();

        return new AdminComplaintDto
        {
            TotalComplaints = enriched.Count,
            OpenComplaints = enriched.Count(c => c.Complaint.Status == "OPEN"),
            CriticalComplaints = enriched.Count(c => c.Meta.Priority == "CRITICAL"),
            ProcessingComplaints = enriched.Count(c => c.Complaint.Status == "PROCESSING"),
            Complaints = enriched.Select(c => MapComplaintItem(c.Complaint, c.Meta)).ToList(),
            SelectedComplaint = selected == null ? null : MapComplaintDetail(selected.Complaint, selected.Meta)
        };
    }

    public async Task<AdminComplaintDto> GetComplaintsAsync(string? category, string? priority, string? status, int? selectedComplaintId)
    {
        var complaints = await _complaintRepository.GetAllAsync();
        var logs = await _systemLogRepository.GetRecentAsync(1000);

        var enriched = complaints
            .Select(c => new { Complaint = c, Meta = BuildComplaintMeta(c, logs) })
            .AsEnumerable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(category) && category != "ALL")
            enriched = enriched.Where(c => c.Meta.Category == category);

        if (!string.IsNullOrWhiteSpace(priority) && priority != "ALL")
            enriched = enriched.Where(c => c.Meta.Priority == priority);

        if (!string.IsNullOrWhiteSpace(status) && status != "ALL")
            enriched = enriched.Where(c => c.Complaint.Status == status);

        var filtered = enriched.ToList();

        var selected = selectedComplaintId.HasValue
            ? filtered.FirstOrDefault(c => c.Complaint.ComplaintId == selectedComplaintId)
            : filtered.FirstOrDefault();

        return new AdminComplaintDto
        {
            TotalComplaints = filtered.Count,
            OpenComplaints = filtered.Count(c => c.Complaint.Status == "OPEN"),
            CriticalComplaints = filtered.Count(c => c.Meta.Priority == "CRITICAL"),
            ProcessingComplaints = filtered.Count(c => c.Complaint.Status == "PROCESSING"),
            Complaints = filtered.OrderByDescending(c => c.Complaint.CreatedAt).Select(c => MapComplaintItem(c.Complaint, c.Meta)).ToList(),
            SelectedComplaint = selected == null ? null : MapComplaintDetail(selected.Complaint, selected.Meta)
        };
    }

    public async Task<ComplaintDetailDto?> GetComplaintDetailAsync(int complaintId)
    {
        var complaint = await _complaintRepository.GetByIdAsync(complaintId);
        if (complaint == null) return null;

        var logs = await _systemLogRepository.GetRecentAsync(1000);
        var meta = BuildComplaintMeta(complaint, logs);
        return MapComplaintDetail(complaint, meta);
    }

    public async Task<bool> UpdateComplaintAsync(UpdateComplaintDto dto, int adminUserId)
    {
        var complaint = await _complaintRepository.GetByIdAsync(dto.ComplaintId);
        if (complaint == null) return false;

        complaint.Status = dto.NextStatus;
        complaint.ResolutionNote = dto.ResolutionNote;
        complaint.UpdatedAt = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(dto.NextStatus) && (dto.NextStatus == "RESOLVED" || dto.NextStatus == "REJECTED"))
        {
            complaint.ResolvedBy = adminUserId;
            complaint.ResolvedAt = DateTime.Now;
        }

        await _complaintRepository.UpdateAsync(complaint);
        await LogActionAsync(adminUserId,
            "UPDATE_COMPLAINT",
            $"Updated complaint #{complaint.ComplaintId}: category={NormalizeCategory(dto.Category)}, priority={NormalizePriority(dto.Priority)}, assignedTo={dto.AssignedToAdminId?.ToString() ?? "AUTO"}",
            "INFO");
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EscalateComplaintAsync(int complaintId, int adminUserId, string escalationReason)
    {
        var complaint = await _complaintRepository.GetByIdAsync(complaintId);
        if (complaint == null) return false;

        complaint.Status = "ESCALATED";
        complaint.UpdatedAt = DateTime.Now;

        await _complaintRepository.UpdateAsync(complaint);
        await LogActionAsync(adminUserId,
            "ESCALATE_COMPLAINT",
            $"Escalated complaint #{complaint.ComplaintId}: {escalationReason}",
            "WARNING");

        var reminder = DateTime.Now.AddDays(1);
        await LogActionAsync(adminUserId,
            "SET_COMPLAINT_REMINDER",
            $"Reminder complaint #{complaint.ComplaintId} at {reminder:O}",
            "INFO");

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetFollowUpReminderAsync(int complaintId, DateTime reminderDate)
    {
        var complaint = await _complaintRepository.GetByIdAsync(complaintId);
        if (complaint == null) return false;

        await LogActionAsync(complaint.ResolvedBy ?? complaint.PatientId,
            "SET_COMPLAINT_REMINDER",
            $"Reminder complaint #{complaint.ComplaintId} at {reminderDate:O}",
            "INFO");
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AutoAssignComplaintAsync(int complaintId, string category, string priority)
    {
        var complaint = await _complaintRepository.GetByIdAsync(complaintId);
        if (complaint == null) return false;

        // Simple auto-assignment logic based on priority and category
        // In production, this could be more sophisticated (round-robin, workload-based, etc)
        var adminUsers = await _userRepository.GetAllAsync(); // Get all admin users
        var availableAdmins = adminUsers.Where(u => u.Role?.RoleName == "ADMIN" && u.IsActive).ToList();

        if (availableAdmins.Count == 0) return false;

        // Assign to first available admin (simplified heuristic)
        var assignedAdmin = availableAdmins.First();
        await LogActionAsync(assignedAdmin.UserId,
            "AUTO_ASSIGN_COMPLAINT",
            $"Assigned complaint #{complaint.ComplaintId} to admin {assignedAdmin.UserId} ({assignedAdmin.FullName}) [category={NormalizeCategory(category)}, priority={NormalizePriority(priority)}]",
            "INFO");
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResolveComplaintAsync(int complaintId, int adminUserId, string resolutionNote, string nextStatus)
    {
        var complaint = await _complaintRepository.GetByIdAsync(complaintId);
        if (complaint == null) return false;

        complaint.ResolutionNote = resolutionNote;
        complaint.Status = nextStatus;
        complaint.ResolvedBy = adminUserId;
        complaint.ResolvedAt = DateTime.Now;
        complaint.UpdatedAt = DateTime.Now;

        await _complaintRepository.UpdateAsync(complaint);
        await LogActionAsync(adminUserId, "RESOLVE_COMPLAINT", $"Resolved complaint #{complaintId}", "INFO");
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<AdminMailNotificationDto> GetMailNotificationAsync()
    {
        var messages = await _notificationRepository.GetRecentByChannelAsync("EMAIL", 50);

        var total = messages.Count;
        var delivered = messages.Count(m => m.Status is "SENT" or "DELIVERED");
        var read = messages.Count(m => m.IsRead);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentMonthAppointments = await _appointmentRepository.GetByDateRangeAsync(
            new DateOnly(today.Year, today.Month, 1),
            new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month)));

        var prevMonthDate = DateTime.Today.AddMonths(-1);
        var prevMonthAppointments = await _appointmentRepository.GetByDateRangeAsync(
            new DateOnly(prevMonthDate.Year, prevMonthDate.Month, 1),
            new DateOnly(prevMonthDate.Year, prevMonthDate.Month, DateTime.DaysInMonth(prevMonthDate.Year, prevMonthDate.Month)));

        var currentNoShow = currentMonthAppointments.Count(a => a.Status == AppointmentStatus.NoShow);
        var prevNoShow = prevMonthAppointments.Count(a => a.Status == AppointmentStatus.NoShow);

        var reduction = prevNoShow == 0
            ? 0m
            : Math.Round((prevNoShow - currentNoShow) * 100m / prevNoShow, 2);

        return new AdminMailNotificationDto
        {
            TotalMessages = total,
            SuccessRatePercent = total == 0 ? 0 : Math.Round(delivered * 100m / total, 2),
            OpenRatePercent = total == 0 ? 0 : Math.Round(read * 100m / total, 2),
            NoShowReductionPercent = reduction,
            Messages = messages.Select(m => new MailMessageItemDto
            {
                NotificationId = m.NotificationId,
                UserName = m.User.FullName,
                Title = m.Title,
                Body = m.Body,
                Channel = m.Channel,
                Status = m.Status,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt
            }).ToList()
        };
    }

    public async Task<int> SendMailNotificationAsync(string targetRole, int? targetUserId, string title, string body, string notificationType)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
        {
            return 0;
        }

        var normalizedRole = (targetRole ?? string.Empty).Trim().ToUpperInvariant();
        if (normalizedRole is not (RoleNames.Patient or RoleNames.Doctor or "ALL"))
        {
            normalizedRole = RoleNames.Patient;
        }

        var normalizedType = string.IsNullOrWhiteSpace(notificationType)
            ? "SYSTEM"
            : notificationType.Trim().ToUpperInvariant();

        var recipients = new List<User>();

        if (targetUserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(targetUserId.Value);
            if (user != null && user.IsActive)
            {
                var isAllowedRole = normalizedRole == "ALL"
                    || string.Equals(user.Role.RoleName, normalizedRole, StringComparison.OrdinalIgnoreCase);

                if (isAllowedRole)
                {
                    recipients.Add(user);
                }
            }
        }
        else if (normalizedRole == "ALL")
        {
            var patients = await _userRepository.SearchUsersAsync(null, RoleNames.Patient, 1000);
            var doctors = await _userRepository.SearchUsersAsync(null, RoleNames.Doctor, 1000);

            recipients = patients
                .Concat(doctors)
                .Where(u => u.IsActive)
                .GroupBy(u => u.UserId)
                .Select(g => g.First())
                .ToList();
        }
        else
        {
            recipients = (await _userRepository.SearchUsersAsync(null, normalizedRole, 1000))
                .Where(u => u.IsActive)
                .ToList();
        }

        if (recipients.Count == 0)
        {
            return 0;
        }

        var now = DateTime.Now;
        var sentSuccessCount = 0;
        var templateContent = LoadMailTemplate();

        foreach (var user in recipients)
        {
            var recipientEmail = user.Email?.Trim();
            EmailSendResult sendResult;
            var normalizedTitle = title.Trim();
            var normalizedBody = body.Trim();

            if (string.IsNullOrWhiteSpace(recipientEmail))
            {
                sendResult = new EmailSendResult
                {
                    Success = false,
                    ErrorMessage = "Missing recipient email."
                };
            }
            else
            {
                var renderedBody = RenderMailTemplate(templateContent, user, normalizedBody);
                sendResult = await _emailMessagingService.SendAsync(
                    recipientEmail,
                    normalizedTitle,
                    renderedBody,
                    isBodyHtml: true);
            }

            if (sendResult.Success)
            {
                sentSuccessCount++;
            }

            var storedBody = normalizedBody;
            if (!sendResult.Success && !string.IsNullOrWhiteSpace(sendResult.ErrorMessage))
            {
                storedBody = $"{storedBody}\n[DeliveryError] {sendResult.ErrorMessage}";
            }

            await _notificationRepository.CreateAsync(new Notification
            {
                UserId = user.UserId,
                AppointmentId = null,
                NotificationType = normalizedType,
                Channel = "EMAIL",
                Title = normalizedTitle,
                Body = storedBody,
                IsRead = false,
                SentAt = sendResult.Success ? now : null,
                ReadAt = null,
                Status = sendResult.Success ? "SENT" : "FAILED",
                CreatedAt = now
            });
        }

        await _unitOfWork.SaveChangesAsync();
        return sentSuccessCount;
    }

    private static string RenderMailTemplate(string template, User user, string notificationBody)
    {
        var safePatientName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(user.FullName) ? "ban" : user.FullName.Trim());
        var safeNotificationBody = WebUtility.HtmlEncode(notificationBody).Replace("\n", "<br />");

        return template
            .Replace("{{patient_name}}", safePatientName, StringComparison.Ordinal)
            .Replace("{{notification_body}}", safeNotificationBody, StringComparison.Ordinal)
            .Replace("{{action_url}}", "#", StringComparison.Ordinal)
            .Replace("{{appointment_time}}", "Dang cap nhat", StringComparison.Ordinal)
            .Replace("{{doctor_name}}", "MediConnect", StringComparison.Ordinal)
            .Replace("{{specialty_name}}", "Dang cap nhat", StringComparison.Ordinal)
            .Replace("{{clinic_location}}", "Dang cap nhat", StringComparison.Ordinal);
    }

    private static string LoadMailTemplate()
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "emailtemplate.html");
        return File.Exists(templatePath)
            ? File.ReadAllText(templatePath)
            : GetFallbackTemplate();
    }

    private static string GetFallbackTemplate()
    {
        return """
               <html>
               <body style="font-family: Arial, Helvetica, sans-serif; color: #12314a;">
                 <h2>MediConnect</h2>
                 <p>Xin chao {{patient_name}},</p>
                 <p>{{notification_body}}</p>
               </body>
               </html>
               """;
    }

    private static ComplaintItemDto MapComplaintItem(Complaint complaint, ComplaintMeta meta)
    {
        return new ComplaintItemDto
        {
            ComplaintId = complaint.ComplaintId,
            Subject = complaint.Subject,
            Description = complaint.Description,
            Status = complaint.Status,
            Category = meta.Category,
            Priority = meta.Priority,
            EscalationLevel = meta.EscalationLevel,
            PatientName = complaint.Patient.FullName,
            DoctorName = complaint.Doctor?.FullName,
            AssignedToName = meta.AssignedToName,
            CreatedAt = complaint.CreatedAt,
            FollowUpReminderDate = meta.FollowUpReminderDate,
            ResolutionNote = complaint.ResolutionNote
        };
    }

    private static ComplaintDetailDto MapComplaintDetail(Complaint complaint, ComplaintMeta meta)
    {
        return new ComplaintDetailDto
        {
            ComplaintId = complaint.ComplaintId,
            Subject = complaint.Subject,
            Description = complaint.Description,
            Status = complaint.Status,
            Category = meta.Category,
            Priority = meta.Priority,
            EscalationLevel = meta.EscalationLevel,
            PatientId = complaint.PatientId,
            PatientName = complaint.Patient.FullName,
            DoctorId = complaint.DoctorId,
            DoctorName = complaint.Doctor?.FullName,
            AssignedToAdminId = meta.AssignedToAdminId,
            AssignedToName = meta.AssignedToName,
            FollowUpReminderDate = meta.FollowUpReminderDate,
            CreatedAt = complaint.CreatedAt,
            UpdatedAt = complaint.UpdatedAt,
            ResolvedAt = complaint.ResolvedAt,
            ResolutionNote = complaint.ResolutionNote
        };
    }

    private sealed record ComplaintMeta(
        string Category,
        string Priority,
        int EscalationLevel,
        int? AssignedToAdminId,
        string? AssignedToName,
        DateTime? FollowUpReminderDate);

    private static ComplaintMeta BuildComplaintMeta(Complaint complaint, List<SystemLog> logs)
    {
        var complaintLogs = logs
            .Where(l => !string.IsNullOrWhiteSpace(l.Description)
                && l.Description!.Contains($"#{complaint.ComplaintId}", StringComparison.Ordinal))
            .OrderByDescending(l => l.CreatedAt)
            .ToList();

        var category = InferCategory(complaint.Subject, complaint.Description);
        var priority = InferPriority(complaint.Subject, complaint.Description, complaint.CreatedAt, complaint.Status);

        var updateLog = complaintLogs.FirstOrDefault(l => l.Action == "UPDATE_COMPLAINT");
        if (updateLog?.Description != null)
        {
            category = ExtractToken(updateLog.Description, "category=") ?? category;
            priority = ExtractToken(updateLog.Description, "priority=") ?? priority;
        }

        var escalationLevel = complaintLogs.Count(l => l.Action == "ESCALATE_COMPLAINT");

        int? assignedAdminId = null;
        string? assignedToName = null;
        var assignLog = complaintLogs.FirstOrDefault(l => l.Action == "AUTO_ASSIGN_COMPLAINT");
        if (assignLog != null)
        {
            assignedAdminId = assignLog.UserId;
            assignedToName = assignLog.User?.FullName;
        }

        DateTime? reminder = null;
        var reminderLog = complaintLogs.FirstOrDefault(l => l.Action == "SET_COMPLAINT_REMINDER");
        if (!string.IsNullOrWhiteSpace(reminderLog?.Description))
        {
            var token = ExtractToken(reminderLog.Description!, "at ");
            if (DateTime.TryParse(token, out var parsed))
            {
                reminder = parsed;
            }
        }

        return new ComplaintMeta(
            NormalizeCategory(category),
            NormalizePriority(priority),
            escalationLevel,
            assignedAdminId,
            assignedToName,
            reminder);
    }

    private static string InferCategory(string subject, string description)
    {
        var text = $"{subject} {description}".ToLowerInvariant();
        if (text.Contains("payment") || text.Contains("thanh toan") || text.Contains("hoa don")) return "PAYMENT";
        if (text.Contains("behavior") || text.Contains("thai do") || text.Contains("ung xu")) return "BEHAVIOR";
        if (text.Contains("chat luong") || text.Contains("quality")) return "QUALITY";
        if (text.Contains("service") || text.Contains("dich vu")) return "SERVICE";
        return "OTHER";
    }

    private static string InferPriority(string subject, string description, DateTime createdAt, string status)
    {
        var text = $"{subject} {description}".ToLowerInvariant();
        if (text.Contains("urgent") || text.Contains("khancap") || text.Contains("nghiem trong")) return "CRITICAL";
        if (DateTime.Now.Subtract(createdAt).TotalHours > 48 && status != "RESOLVED") return "HIGH";
        if (text.Contains("delay") || text.Contains("cham")) return "HIGH";
        return "NORMAL";
    }

    private static string NormalizeCategory(string? category)
    {
        var value = (category ?? "OTHER").Trim().ToUpperInvariant();
        return value is "QUALITY" or "PAYMENT" or "BEHAVIOR" or "SERVICE" ? value : "OTHER";
    }

    private static string NormalizePriority(string? priority)
    {
        var value = (priority ?? "NORMAL").Trim().ToUpperInvariant();
        return value is "CRITICAL" or "HIGH" or "NORMAL" or "LOW" ? value : "NORMAL";
    }

    private static string? ExtractToken(string text, string prefix)
    {
        var index = text.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (index < 0) return null;

        var start = index + prefix.Length;
        var end = text.IndexOfAny(new[] { ',', ']', ' ' }, start);
        if (end < 0) end = text.Length;

        var token = text[start..end].Trim();
        return string.IsNullOrWhiteSpace(token) ? null : token;
    }

    // New User Management Methods
    public async Task<UserDetailDto?> GetUserDetailAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        var activities = await _systemLogRepository.GetByUserIdAsync(userId, 50);

        return new UserDetailDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Address = user.Address,
            RoleName = user.Role?.RoleName ?? "UNKNOWN",
            IsActive = user.IsActive,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            RecentActivities = activities.Select(a => new UserActivityItemDto
            {
                LogId = a.LogId,
                Action = a.Action,
                Description = a.Description,
                Severity = a.Severity,
                CreatedAt = a.CreatedAt
            }).ToList()
        };
    }

    public async Task<bool> ToggleUserStatusAsync(int userId, bool isActive, int adminUserId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        var wasActive = user.IsActive;
        user.IsActive = isActive;
        user.UpdatedAt = DateTime.Now;

        await _userRepository.UpdateAsync(user);

        var action = isActive ? "ENABLE_USER" : "DISABLE_USER";
        var description = $"User {user.Email} {(isActive ? "enabled" : "disabled")} by admin";
        await LogActionAsync(adminUserId, action, description, "INFO");

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserAsync(EditUserDto dto, int adminUserId)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user == null) return false;

        var changes = new List<string>();
        if (user.FullName != dto.FullName)
        {
            changes.Add($"Name: {user.FullName} → {dto.FullName}");
            user.FullName = dto.FullName;
        }
        if (user.Email != dto.Email)
        {
            changes.Add($"Email: {user.Email} → {dto.Email}");
            user.Email = dto.Email;
        }
        if (user.PhoneNumber != dto.PhoneNumber)
        {
            changes.Add($"Phone: {user.PhoneNumber} → {dto.PhoneNumber}");
            user.PhoneNumber = dto.PhoneNumber;
        }
        if (user.Gender != dto.Gender)
        {
            changes.Add($"Gender: {user.Gender} → {dto.Gender}");
            user.Gender = dto.Gender;
        }
        if (user.DateOfBirth != dto.DateOfBirth)
        {
            changes.Add($"DOB: {user.DateOfBirth} → {dto.DateOfBirth}");
            user.DateOfBirth = dto.DateOfBirth;
        }
        if (user.Address != dto.Address)
        {
            changes.Add($"Address: {user.Address} → {dto.Address}");
            user.Address = dto.Address;
        }

        if (changes.Count == 0) return true;

        user.UpdatedAt = DateTime.Now;
        await _userRepository.UpdateAsync(user);

        var description = $"User {user.Email} updated. Changes: {string.Join("; ", changes)}";
        await LogActionAsync(adminUserId, "EDIT_USER", description, "INFO");

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetUserPasswordAsync(int userId, int adminUserId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        if (string.IsNullOrWhiteSpace(user.Email)) return false;

        var tempPassword = GenerateTemporaryPassword();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
        user.PasswordHash = passwordHash;
        user.UpdatedAt = DateTime.Now;

        await _userRepository.UpdateAsync(user);

        var subject = "[MediConnect] Mat khau tam thoi cua ban";
        var body = $"""
Xin chao {user.FullName},

Tai khoan MediConnect cua ban vua duoc dat lai mat khau boi quan tri vien.

Mat khau tam thoi: {tempPassword}

Vui long dang nhap va doi mat khau ngay de dam bao an toan.
Neu ban khong yeu cau thao tac nay, vui long lien he ho tro ngay.

Tran trong,
MediConnect
""";

        var sendResult = await _emailMessagingService.SendAsync(user.Email.Trim(), subject, body, isBodyHtml: false);

        var description = sendResult.Success
            ? $"Password reset for user {user.Email}. Temp password sent to email."
            : $"Password reset for user {user.Email} but email delivery failed: {sendResult.ErrorMessage}";

        await LogActionAsync(adminUserId, "RESET_PASSWORD", description, sendResult.Success ? "WARNING" : "ERROR");
        await _unitOfWork.SaveChangesAsync();
        return sendResult.Success;
    }

    public async Task<List<UserActivityItemDto>> GetUserActivityAsync(int userId, int limit = 50)
    {
        var activities = await _systemLogRepository.GetByUserIdAsync(userId, limit);
        return activities.Select(a => new UserActivityItemDto
        {
            LogId = a.LogId,
            Action = a.Action,
            Description = a.Description,
            Severity = a.Severity,
            CreatedAt = a.CreatedAt
        }).ToList();
    }

    public async Task<byte[]> ExportUsersExcelAsync(List<int>? userIds = null)
    {
        List<User> users;
        if (userIds != null && userIds.Count > 0)
        {
            users = new List<User>();
            foreach (var userId in userIds)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null) users.Add(user);
            }
        }
        else
        {
            users = await _userRepository.GetAllAsync();
        }

        using (var workbook = new OfficeOpenXml.ExcelPackage())
        {
            var worksheet = workbook.Workbook.Worksheets.Add("Users");

            // Headers
            worksheet.Cells[1, 1].Value = "User ID";
            worksheet.Cells[1, 2].Value = "Full Name";
            worksheet.Cells[1, 3].Value = "Email";
            worksheet.Cells[1, 4].Value = "Phone";
            worksheet.Cells[1, 5].Value = "Role";
            worksheet.Cells[1, 6].Value = "Status";
            worksheet.Cells[1, 7].Value = "Created At";
            worksheet.Cells[1, 8].Value = "Last Login";

            // Data
            int row = 2;
            foreach (var user in users)
            {
                worksheet.Cells[row, 1].Value = user.UserId;
                worksheet.Cells[row, 2].Value = user.FullName;
                worksheet.Cells[row, 3].Value = user.Email;
                worksheet.Cells[row, 4].Value = user.PhoneNumber;
                worksheet.Cells[row, 5].Value = user.Role.RoleName;
                worksheet.Cells[row, 6].Value = user.IsActive ? "Active" : "Inactive";
                worksheet.Cells[row, 7].Value = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cells[row, 8].Value = user.LastLoginAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never";
                row++;
            }

            // Auto-fit columns
            worksheet.Column(1).Width = 12;
            worksheet.Column(2).Width = 25;
            worksheet.Column(3).Width = 25;
            worksheet.Column(4).Width = 15;
            worksheet.Column(5).Width = 12;
            worksheet.Column(6).Width = 12;
            worksheet.Column(7).Width = 20;
            worksheet.Column(8).Width = 20;

            return workbook.GetAsByteArray();
        }
    }

    private async Task LogActionAsync(int userId, string action, string description, string severity)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return;

        var log = new SystemLog
        {
            UserId = userId,
            Action = action,
            Description = description,
            Severity = severity,
            CreatedAt = DateTime.Now,
            IpAddress = "SYSTEM"
        };

        await _systemLogRepository.CreateAsync(log);
    }

    private static string GenerateTemporaryPassword()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
        var random = new Random();
        var password = new System.Text.StringBuilder();
        for (int i = 0; i < 12; i++)
        {
            password.Append(chars[random.Next(chars.Length)]);
        }
        return password.ToString();
    }
}
