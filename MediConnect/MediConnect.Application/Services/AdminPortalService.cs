using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using MediConnect.Domain.Entities;
using System.Net;

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
        var selected = selectedComplaintId.HasValue
            ? complaints.FirstOrDefault(c => c.ComplaintId == selectedComplaintId)
            : complaints.FirstOrDefault();

        return new AdminComplaintDto
        {
            TotalComplaints = complaints.Count,
            OpenComplaints = complaints.Count(c => c.Status == "OPEN"),
            ProcessingComplaints = complaints.Count(c => c.Status == "PROCESSING"),
            Complaints = complaints.Select(MapComplaint).ToList(),
            SelectedComplaint = selected == null ? null : MapComplaint(selected)
        };
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

    private static ComplaintItemDto MapComplaint(Complaint complaint)
    {
        return new ComplaintItemDto
        {
            ComplaintId = complaint.ComplaintId,
            Subject = complaint.Subject,
            Description = complaint.Description,
            Status = complaint.Status,
            PatientName = complaint.Patient.FullName,
            DoctorName = complaint.Doctor?.FullName,
            CreatedAt = complaint.CreatedAt,
            ResolutionNote = complaint.ResolutionNote
        };
    }
}
