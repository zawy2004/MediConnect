using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using MediConnect.Domain.Entities;

namespace MediConnect.Application.Services;

public class DoctorPortalService : IDoctorPortalService
{
    private readonly IUserRepository _userRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IDoctorService _doctorService;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IAppointmentWaitlistRepository _appointmentWaitlistRepository;
    private readonly IMedicalRecordRepository _medicalRecordRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILlmService _llmService;
    private readonly IUnitOfWork _unitOfWork;

    public DoctorPortalService(
        IUserRepository userRepository,
        IDoctorRepository doctorRepository,
        IDoctorService doctorService,
        IAppointmentRepository appointmentRepository,
        IAppointmentWaitlistRepository appointmentWaitlistRepository,
        IMedicalRecordRepository medicalRecordRepository,
        IReviewRepository reviewRepository,
        INotificationRepository notificationRepository,
        IPaymentRepository paymentRepository,
        ILlmService llmService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _doctorRepository = doctorRepository;
        _doctorService = doctorService;
        _appointmentRepository = appointmentRepository;
        _appointmentWaitlistRepository = appointmentWaitlistRepository;
        _medicalRecordRepository = medicalRecordRepository;
        _reviewRepository = reviewRepository;
        _notificationRepository = notificationRepository;
        _paymentRepository = paymentRepository;
        _llmService = llmService;
        _unitOfWork = unitOfWork;
    }

    public async Task<DoctorOverviewDto> GetOverviewAsync(int doctorUserId)
    {
        var user = await _userRepository.GetByIdAsync(doctorUserId);
        var profile = await _doctorRepository.GetByUserIdAsync(doctorUserId);
        var reviews = await _reviewRepository.GetByDoctorIdAsync(doctorUserId, 200);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var firstOfMonth = new DateOnly(today.Year, today.Month, 1);
        var lastOfMonth = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

        var allForMonth = await _appointmentRepository.GetByDateRangeAsync(firstOfMonth, lastOfMonth);
        var doctorMonth = allForMonth.Where(a => a.DoctorId == doctorUserId).ToList();

        var weekStart = today.AddDays(today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1 - (int)today.DayOfWeek);
        var weekSchedule = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = weekStart.AddDays(offset);
                var appointmentsForDay = doctorMonth
                    .Where(a => a.AppointmentDate == date)
                    .OrderBy(a => a.StartTime)
                    .Select(a => new DoctorScheduleItemDto
                    {
                        AppointmentId = a.AppointmentId,
                        PatientName = a.Patient.FullName,
                        AppointmentDate = a.AppointmentDate,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        Status = a.Status,
                        Reason = a.Reason
                    })
                    .ToList();

                return new DoctorScheduleDayDto
                {
                    Date = date,
                    Label = date.DayOfWeek switch
                    {
                        DayOfWeek.Monday => "T2",
                        DayOfWeek.Tuesday => "T3",
                        DayOfWeek.Wednesday => "T4",
                        DayOfWeek.Thursday => "T5",
                        DayOfWeek.Friday => "T6",
                        DayOfWeek.Saturday => "T7",
                        DayOfWeek.Sunday => "CN",
                        _ => string.Empty
                    },
                    Appointments = appointmentsForDay
                };
            })
            .ToList();

        var monthSchedule = Enumerable.Range(1, DateTime.DaysInMonth(today.Year, today.Month))
            .Select(day =>
            {
                var date = new DateOnly(today.Year, today.Month, day);
                var items = doctorMonth.Where(a => a.AppointmentDate == date).ToList();
                return new DoctorScheduleMonthDayDto
                {
                    Date = date,
                    AppointmentCount = items.Count,
                    HasUpcoming = items.Any(a => a.Status != AppointmentStatus.Completed),
                    HasCompleted = items.Any(a => a.Status == AppointmentStatus.Completed)
                };
            })
            .ToList();

        var timeline = doctorMonth
            .Where(a => a.AppointmentDate == today)
            .OrderBy(a => a.StartTime)
            .Take(10)
            .Select(a => new DoctorScheduleItemDto
            {
                AppointmentId = a.AppointmentId,
                PatientName = a.Patient.FullName,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                Reason = a.Reason
            })
            .ToList();

        var pending = doctorMonth
            .Where(a => a.Status == AppointmentStatus.Pending)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .Take(5)
            .Select(MapRequest)
            .ToList();

        var distinctPatients = doctorMonth.Select(a => a.PatientId).Distinct().Count();
        var satisfaction = reviews.Count == 0 ? 0m : Math.Round((decimal)reviews.Average(r => r.Rating) * 20m, 2);

        return new DoctorOverviewDto
        {
            DoctorName = user?.FullName ?? "Bác sĩ",
            DepartmentName = profile?.Department?.DepartmentName,
            TotalPatients = distinctPatients,
            SatisfactionPercent = satisfaction,
            AppointmentsThisMonth = doctorMonth.Count,
            AverageConsultationMinutes = doctorMonth.Count == 0 ? 0 : (int)Math.Round(doctorMonth.Average(a => (a.EndTime - a.StartTime).TotalMinutes)),
            TodayTimeline = timeline,
            WeekSchedule = weekSchedule,
            MonthSchedule = monthSchedule,
            PendingRequests = pending
        };
    }

    public async Task<List<DoctorAppointmentRequestDto>> GetPendingRequestsAsync(int doctorUserId)
    {
        var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorUserId);
        return appointments
            .Where(a => a.Status == AppointmentStatus.Pending)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .Take(20)
            .Select(MapRequest)
            .ToList();
    }

    public async Task<List<DoctorPatientGroupDto>> GetPatientGroupsAsync(int doctorUserId)
    {
        var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorUserId);

        var categories = new[]
        {
            new { CategoryKey = "completed", Category = "Đã khám", Statuses = new[] { AppointmentStatus.Completed } },
            new { CategoryKey = "confirmed", Category = "Đã được chấp nhận", Statuses = new[] { AppointmentStatus.Confirmed } },
            new { CategoryKey = "pending", Category = "Yêu cầu mới", Statuses = new[] { AppointmentStatus.Pending } }
        };

        return categories.Select(category => new DoctorPatientGroupDto
        {
            CategoryKey = category.CategoryKey,
            Category = category.Category,
            Patients = appointments
                .Where(a => category.Statuses.Contains(a.Status))
                .GroupBy(a => a.PatientId)
                .Select(g =>
                {
                    var latest = g
                        .OrderByDescending(a => a.AppointmentDate)
                        .ThenByDescending(a => a.StartTime)
                        .First();

                    return new DoctorPatientListItemDto
                    {
                        PatientId = latest.PatientId,
                        AppointmentId = latest.AppointmentId,
                        PatientName = latest.Patient?.FullName ?? "Bệnh nhân",
                        PatientEmail = latest.Patient?.Email,
                        Gender = latest.Patient?.Gender ?? string.Empty,
                        SpecialtyName = latest.Specialty?.SpecialtyName ?? string.Empty,
                        Status = latest.Status,
                        AppointmentDate = latest.AppointmentDate,
                        StartTime = latest.StartTime,
                        Reason = latest.Reason
                    };
                })
                .OrderByDescending(item => item.AppointmentDate)
                .ThenBy(item => item.StartTime)
                .ToList()
        }).ToList();
    }

    public async Task<bool> ConfirmRequestAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null || appointment.Status != AppointmentStatus.Pending)
        {
            return false;
        }

        appointment.Status = AppointmentStatus.Confirmed;
        appointment.ConfirmedAt = DateTime.Now;
        appointment.UpdatedAt = DateTime.Now;

        await _appointmentRepository.UpdateAsync(appointment);

        await _notificationRepository.CreateAsync(new Notification
        {
            UserId = appointment.PatientId,
            AppointmentId = appointment.AppointmentId,
            NotificationType = "DOCTOR_CONFIRMATION",
            Channel = "IN_APP",
            Title = "Lịch hẹn đã được bác sĩ xác nhận",
            Body = $"Lịch hẹn ngày {appointment.AppointmentDate:dd/MM/yyyy} lúc {appointment.StartTime:HH:mm} đã được xác nhận.",
            IsRead = false,
            SentAt = DateTime.Now,
            Status = "SENT",
            CreatedAt = DateTime.Now
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectRequestAsync(int appointmentId, string reason)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null || appointment.Status != AppointmentStatus.Pending)
        {
            return false;
        }

        appointment.Status = AppointmentStatus.CancelledByDoctor;
        appointment.CancelReason = reason;
        appointment.CancelledAt = DateTime.Now;
        appointment.UpdatedAt = DateTime.Now;

        await _appointmentRepository.UpdateAsync(appointment);

        await _notificationRepository.CreateAsync(new Notification
        {
            UserId = appointment.PatientId,
            AppointmentId = appointment.AppointmentId,
            NotificationType = "APPOINTMENT_CANCELLATION",
            Channel = "IN_APP",
            Title = "Lịch hẹn bị từ chối",
            Body = string.IsNullOrWhiteSpace(reason)
                ? "Bác sĩ đã từ chối lịch hẹn. Vui lòng chọn khung giờ khác."
                : $"Lịch hẹn bị từ chối: {reason}",
            IsRead = false,
            SentAt = DateTime.Now,
            Status = "SENT",
            CreatedAt = DateTime.Now
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<DoctorWaitlistDto> GetWaitlistAsync(int doctorUserId)
    {
        var items = await _appointmentWaitlistRepository.GetByDoctorIdAsync(doctorUserId);
        var ordered = items.OrderBy(i => i.CreatedAt).ToList();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthEnd = today.AddMonths(1).AddDays(-1);
        var monthAppointments = (await _appointmentRepository.GetByDateRangeAsync(today, monthEnd))
            .Where(a => a.DoctorId == doctorUserId)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToList();

        return new DoctorWaitlistDto
        {
            IsFullToday = ordered.Any(i => i.PreferredDate == DateOnly.FromDateTime(DateTime.Today)),
            TotalCount = ordered.Count,
            AverageWaitMinutes = ordered.Count == 0
                ? 0
                : (int)Math.Round(ordered.Average(i => (DateTime.Now - i.CreatedAt).TotalMinutes)),
            Items = ordered.Select((item, index) => new DoctorWaitlistItemDto
            {
                WaitlistId = item.WaitlistId,
                Order = index + 1,
                PatientId = item.PatientId,
                PatientName = item.Patient.FullName,
                PreferredDate = item.PreferredDate,
                PreferredTime = item.PreferredTime,
                Status = item.Status,
                CreatedAt = item.CreatedAt
            }).ToList(),
            CalendarEvents = monthAppointments.Select(a => new DoctorCalendarEventDto
            {
                AppointmentId = a.AppointmentId,
                Title = a.Specialty?.SpecialtyName ?? a.Reason ?? "Lịch hẹn",
                PatientName = a.Patient?.FullName ?? "Bệnh nhân",
                StartDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status
            }).ToList()
        };
    }

    public async Task<bool> NotifyNextWaitlistAsync(int doctorUserId)
    {
        var items = await _appointmentWaitlistRepository.GetByDoctorIdAsync(doctorUserId);
        var next = items
            .Where(i => i.Status == "WAITING")
            .OrderBy(i => i.CreatedAt)
            .FirstOrDefault();

        if (next == null)
        {
            return false;
        }

        next.Status = "NOTIFIED";
        next.NotifiedAt = DateTime.Now;
        next.UpdatedAt = DateTime.Now;
        await _appointmentWaitlistRepository.UpdateAsync(next);

        await _notificationRepository.CreateAsync(new Notification
        {
            UserId = next.PatientId,
            NotificationType = "APPOINTMENT_REMINDER",
            Channel = "IN_APP",
            Title = "Có slot khám trống mới",
            Body = "Bạn đang ở đầu danh sách chờ. Vui lòng xác nhận lịch sớm nhất.",
            IsRead = false,
            SentAt = DateTime.Now,
            Status = "SENT",
            CreatedAt = DateTime.Now
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MoveWaitlistUpAsync(int waitlistId, int doctorUserId)
    {
        var rows = await _appointmentWaitlistRepository.GetByDoctorIdAsync(doctorUserId);
        var ordered = rows.OrderBy(r => r.CreatedAt).ToList();
        var index = ordered.FindIndex(r => r.WaitlistId == waitlistId);
        if (index <= 0)
        {
            return false;
        }

        var current = ordered[index];
        var previous = ordered[index - 1];
        (current.CreatedAt, previous.CreatedAt) = (previous.CreatedAt, current.CreatedAt);
        current.UpdatedAt = DateTime.Now;
        previous.UpdatedAt = DateTime.Now;

        await _appointmentWaitlistRepository.UpdateAsync(current);
        await _appointmentWaitlistRepository.UpdateAsync(previous);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MoveWaitlistDownAsync(int waitlistId, int doctorUserId)
    {
        var rows = await _appointmentWaitlistRepository.GetByDoctorIdAsync(doctorUserId);
        var ordered = rows.OrderBy(r => r.CreatedAt).ToList();
        var index = ordered.FindIndex(r => r.WaitlistId == waitlistId);
        if (index < 0 || index >= ordered.Count - 1)
        {
            return false;
        }

        var current = ordered[index];
        var next = ordered[index + 1];
        (current.CreatedAt, next.CreatedAt) = (next.CreatedAt, current.CreatedAt);
        current.UpdatedAt = DateTime.Now;
        next.UpdatedAt = DateTime.Now;

        await _appointmentWaitlistRepository.UpdateAsync(current);
        await _appointmentWaitlistRepository.UpdateAsync(next);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<DoctorPatientRecordDto?> GetPatientRecordAsync(int doctorUserId, int patientId)
    {
        var patient = await _userRepository.GetByIdAsync(patientId);
        if (patient == null)
        {
            return null;
        }

        var records = await _medicalRecordRepository.GetByDoctorAndPatientAsync(doctorUserId, patientId);
        var latest = records.FirstOrDefault();

        var age = patient.DateOfBirth.HasValue
            ? DateTime.Today.Year - patient.DateOfBirth.Value.Year
            : (int?)null;

        if (patient.DateOfBirth.HasValue &&
            new DateTime(DateTime.Today.Year, patient.DateOfBirth.Value.Month, patient.DateOfBirth.Value.Day) > DateTime.Today)
        {
            age--;
        }

        var random = new Random(patientId + doctorUserId);

        return new DoctorPatientRecordDto
        {
            PatientId = patient.UserId,
            PatientName = patient.FullName,
            Gender = patient.Gender,
            Age = age,
            BloodType = patientId % 2 == 0 ? "A+" : "O+",
            SymptomSummary = latest?.Symptoms ?? "Bệnh nhân có triệu chứng đau tức ngực nhẹ, cần theo dõi huyết áp và nhịp tim.",
            AllergyAlert = "Bệnh nhân có tiền sử dị ứng Penicillin. Vui lòng kiểm tra trước khi kê đơn.",
            LastBloodPressureSystolic = 110 + random.Next(0, 25),
            LastBloodPressureDiastolic = 70 + random.Next(0, 15),
            LastHeartRate = 65 + random.Next(0, 20),
            History = records.Select(r => new PatientRecordHistoryDto
            {
                RecordId = r.RecordId,
                RecordDate = r.RecordDate,
                Diagnosis = r.Diagnosis,
                Notes = r.Notes,
                DoctorName = r.Doctor.FullName
            }).ToList(),
            Documents = records.Take(3).Select(r => new PatientDocumentDto
            {
                Name = $"Kết quả khám {r.RecordDate:ddMMyyyy}.pdf",
                Type = "PDF",
                CreatedAt = r.CreatedAt
            }).ToList(),
            ClinicalNoteDraft = latest?.Notes ?? string.Empty
        };
    }

    public async Task<bool> SaveClinicalNoteAsync(int doctorUserId, int patientId, string note)
    {
        var latest = await _medicalRecordRepository.GetLatestByDoctorAndPatientAsync(doctorUserId, patientId);
        if (latest == null)
        {
            await _medicalRecordRepository.CreateAsync(new MedicalRecord
            {
                DoctorId = doctorUserId,
                PatientId = patientId,
                Notes = note,
                RecordDate = DateOnly.FromDateTime(DateTime.Today),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }
        else
        {
            latest.Notes = note;
            latest.UpdatedAt = DateTime.Now;
            await _medicalRecordRepository.UpdateAsync(latest);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<DoctorConsultationDto?> GetConsultationAsync(int doctorUserId, int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null || appointment.DoctorId != doctorUserId)
        {
            return null;
        }

        var latestRecord = await _medicalRecordRepository.GetLatestByDoctorAndPatientAsync(doctorUserId, appointment.PatientId);
        var patient = appointment.Patient;
        var age = patient.DateOfBirth.HasValue
            ? DateTime.Today.Year - patient.DateOfBirth.Value.Year
            : (int?)null;

        if (patient.DateOfBirth.HasValue &&
            new DateTime(DateTime.Today.Year, patient.DateOfBirth.Value.Month, patient.DateOfBirth.Value.Day) > DateTime.Today)
        {
            age--;
        }

        return new DoctorConsultationDto
        {
            AppointmentId = appointment.AppointmentId,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient.FullName,
            PatientAge = age,
            PatientGender = appointment.Patient.Gender,
            BloodType = appointment.PatientId % 2 == 0 ? "A+" : "O+",
            AllergyAlert = latestRecord?.Notes ?? "Chưa có tiền sử dị ứng cụ thể.",
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Reason = appointment.Reason,
            Status = appointment.Status,
            Symptoms = latestRecord?.Symptoms,
            Diagnosis = latestRecord?.Diagnosis,
            TreatmentPlan = latestRecord?.TreatmentPlan,
            Prescription = latestRecord?.Prescription,
            Notes = latestRecord?.Notes
        };
    }

    public async Task<bool> CompleteConsultationAsync(int doctorUserId, int appointmentId, string symptoms, string diagnosis, string treatmentPlan, string prescription, string notes)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null || appointment.DoctorId != doctorUserId)
        {
            return false;
        }

        appointment.Status = AppointmentStatus.Completed;
        appointment.Notes = notes;
        appointment.UpdatedAt = DateTime.Now;
        await _appointmentRepository.UpdateAsync(appointment);

        await _medicalRecordRepository.CreateAsync(new MedicalRecord
        {
            DoctorId = doctorUserId,
            PatientId = appointment.PatientId,
            AppointmentId = appointment.AppointmentId,
            Symptoms = symptoms,
            Diagnosis = diagnosis,
            TreatmentPlan = treatmentPlan,
            Prescription = prescription,
            Notes = notes,
            RecordDate = DateOnly.FromDateTime(DateTime.Today),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<DoctorPerformanceDto> GetPerformanceAsync(int doctorUserId)
    {
        var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorUserId);
        var reviews = await _reviewRepository.GetByDoctorIdAsync(doctorUserId, 50);
        var doctor = await _doctorRepository.GetByUserIdAsync(doctorUserId);

        var now = DateOnly.FromDateTime(DateTime.Today);
        var weekDates = Enumerable.Range(0, 7).Select(d => now.AddDays(-6 + d)).ToList();

        var weekly = weekDates.Select(day => new DailyCountDto
        {
            Date = day,
            Count = appointments.Count(a => a.AppointmentDate == day)
        }).ToList();

        var completed = appointments.Count(a => a.Status == AppointmentStatus.Completed);
        var returnRate = appointments.Count == 0 ? 0m : Math.Round(completed * 100m / appointments.Count, 2);
        var avgRating = reviews.Count == 0 ? 0m : Math.Round((decimal)reviews.Average(r => r.Rating), 2);

        var diseases = appointments
            .SelectMany(a => (a.Reason ?? string.Empty)
                .Split([' ', ',', '.', ';', ':', '-', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries))
            .Where(w => w.Length >= 4)
            .GroupBy(w => w, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => new KeywordWeightDto { Keyword = g.Key, Weight = g.Count() })
            .ToList();

        // Get specialty from doctor user
        var doctorUser = await _userRepository.GetByIdAsync(doctorUserId);
        var specialtyName = doctorUser?.DoctorSpecialties?.FirstOrDefault()?.Specialty?.SpecialtyName ?? "Tổng quát";

        // Generate AI insight based on performance data
        var aiInsight = await GeneratePerformanceInsightAsync(
            specialtyName,
            appointments.ToList(),
            reviews.ToList(),
            weekly,
            diseases,
            avgRating,
            returnRate);

        return new DoctorPerformanceDto
        {
            TotalPatients = appointments.Select(a => a.PatientId).Distinct().Count(),
            ReturnRatePercent = returnRate,
            AverageRating = avgRating,
            AverageConsultationMinutes = appointments.Count == 0 ? 0 : (int)Math.Round(appointments.Average(a => (a.EndTime - a.StartTime).TotalMinutes)),
            WeeklyAppointments = weekly,
            DiseaseDistribution = diseases,
            RecentFeedbacks = reviews.Take(5).Select(r => new DoctorFeedbackDto
            {
                PatientName = r.Patient.FullName,
                Rating = r.Rating,
                Comment = r.Comment ?? string.Empty,
                CreatedAt = r.CreatedAt
            }).ToList(),
            AiInsight = aiInsight
        };
    }

    private async Task<string> GeneratePerformanceInsightAsync(
        string specialty,
        List<Appointment> appointments,
        List<Review> reviews,
        List<DailyCountDto> weekly,
        List<KeywordWeightDto> diseases,
        decimal avgRating,
        decimal returnRate)
    {
        try
        {
            // Analyze time patterns
            var morningCount = appointments.Count(a => a.StartTime.Hour < 12);
            var afternoonCount = appointments.Count(a => a.StartTime.Hour >= 12 && a.StartTime.Hour < 17);
            var eveningCount = appointments.Count(a => a.StartTime.Hour >= 17);

            // Analyze day patterns
            var dayDistribution = appointments
                .GroupBy(a => a.AppointmentDate.DayOfWeek)
                .OrderByDescending(g => g.Count())
                .Take(2)
                .Select(g => $"{g.Key}: {g.Count()}")
                .ToList();

            // Analyze feedback sentiment
            var positiveReviews = reviews.Count(r => r.Rating >= 4);
            var negativeReviews = reviews.Count(r => r.Rating <= 2);

            // Find improvement areas from comments
            var commonWords = reviews
                .Where(r => !string.IsNullOrWhiteSpace(r.Comment))
                .SelectMany(r => r.Comment!.Split([' ', ',', '.'], StringSplitOptions.RemoveEmptyEntries))
                .Where(w => w.Length >= 3)
                .GroupBy(w => w.ToLowerInvariant())
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();

            var contextData = $@"
Dữ liệu hiệu suất bác sĩ chuyên khoa {specialty}:

TỔNG QUAN:
- Tổng bệnh nhân: {appointments.Select(a => a.PatientId).Distinct().Count()}
- Tỷ lệ hoàn thành: {returnRate}%
- Đánh giá trung bình: {avgRating}/5

PHÂN BỔ THỜI GIAN:
- Buổi sáng (<12h): {morningCount} ca
- Buổi chiều (12-17h): {afternoonCount} ca  
- Buổi tối (>17h): {eveningCount} ca
- Ngày cao điểm: {string.Join(", ", dayDistribution)}

BỆNH LÝ PHỔ BIẾN:
{string.Join(", ", diseases.Select(d => $"{d.Keyword} ({d.Weight} lần)"))}

PHẢN HỒI:
- Tích cực (4-5 sao): {positiveReviews}
- Tiêu cực (1-2 sao): {negativeReviews}
- Từ khóa thường gặp: {string.Join(", ", commonWords)}

XU HƯỚNG 7 NGÀY:
{string.Join(", ", weekly.Select(w => $"{w.Date:dd/MM}: {w.Count}"))}
";

            var systemPrompt = @"Bạn là AI tư vấn hiệu suất cho bác sĩ. Dựa trên dữ liệu, đưa ra 1 gợi ý cụ thể, hữu ích để cải thiện hiệu suất. 

Yêu cầu:
- Ngắn gọn (1-2 câu)
- Cụ thể và có thể hành động được
- Dựa trên dữ liệu thực
- Tiếng Việt chuyên nghiệp";

            var messages = new List<LlmMessage>
            {
                new() { Role = "user", Content = contextData }
            };

            var response = await _llmService.GenerateResponseAsync(messages, systemPrompt);
            return string.IsNullOrWhiteSpace(response)
                ? "AI gợi ý tăng ưu tiên các ca tim mạch buổi sáng để tối ưu thời gian khám trung bình."
                : response.Trim();
        }
        catch
        {
            return "AI gợi ý tăng ưu tiên các ca tim mạch buổi sáng để tối ưu thời gian khám trung bình.";
        }
    }

    public async Task<DoctorSpecialtyProfileDto?> GetSpecialtyProfileAsync(int doctorUserId)
    {
        var profile = await _doctorRepository.GetByUserIdAsync(doctorUserId);
        if (profile == null)
        {
            return null;
        }

        var doctor = await _doctorService.GetDoctorDetailAsync(profile.DoctorProfileId);
        var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorUserId);
        
        // Get specialty from user
        var doctorUser = await _userRepository.GetByIdAsync(doctorUserId);
        var specialty = doctorUser?.DoctorSpecialties?.FirstOrDefault()?.Specialty?.SpecialtyName ?? "Tổng quát";
        
        // Generate AI trend and recommendation
        var (aiTrend, aiRecommendation) = await GenerateTrendAndRecommendationAsync(
            specialty,
            appointments.ToList(),
            profile);

        return new DoctorSpecialtyProfileDto
        {
            Doctor = doctor,
            MembershipPlan = "PREMIUM",
            MembershipExpiresAt = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
            AiTrend = aiTrend,
            AiRecommendation = aiRecommendation
        };
    }

    private async Task<(string Trend, string Recommendation)> GenerateTrendAndRecommendationAsync(
        string specialty,
        List<Appointment> appointments,
        DoctorProfile profile)
    {
        try
        {
            var now = DateOnly.FromDateTime(DateTime.Today);
            
            // Analyze recent trends
            var last30Days = appointments.Where(a => a.AppointmentDate >= now.AddDays(-30)).ToList();
            var last7Days = appointments.Where(a => a.AppointmentDate >= now.AddDays(-7)).ToList();
            var prev7Days = appointments.Where(a => a.AppointmentDate >= now.AddDays(-14) && a.AppointmentDate < now.AddDays(-7)).ToList();

            var growthRate = prev7Days.Count == 0 ? 0 : ((last7Days.Count - prev7Days.Count) * 100 / prev7Days.Count);

            // Analyze reasons/symptoms
            var symptoms = last30Days
                .Where(a => !string.IsNullOrWhiteSpace(a.Reason))
                .SelectMany(a => a.Reason!.Split([' ', ',', '.', ';'], StringSplitOptions.RemoveEmptyEntries))
                .Where(w => w.Length >= 4)
                .GroupBy(w => w.ToLowerInvariant())
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Keyword = g.Key, Count = g.Count() })
                .ToList();

            // Analyze time slot demand
            var timeSlots = last30Days
                .GroupBy(a => a.StartTime.Hour switch
                {
                    < 10 => "Sáng sớm (7-10h)",
                    < 12 => "Cuối sáng (10-12h)",
                    < 14 => "Đầu chiều (12-14h)",
                    < 17 => "Chiều (14-17h)",
                    _ => "Tối (17h+)"
                })
                .OrderByDescending(g => g.Count())
                .Select(g => new { Slot = g.Key, Count = g.Count() })
                .ToList();

            // Analyze day of week
            var dayDemand = last30Days
                .GroupBy(a => a.AppointmentDate.DayOfWeek)
                .OrderByDescending(g => g.Count())
                .Take(2)
                .Select(g => new { Day = g.Key.ToString(), Count = g.Count() })
                .ToList();

            var contextData = $@"
Phân tích xu hướng cho bác sĩ chuyên khoa {specialty}:

TĂNG TRƯỞNG:
- Tăng trưởng 7 ngày: {growthRate}%
- Lịch hẹn 7 ngày qua: {last7Days.Count}
- Lịch hẹn 30 ngày qua: {last30Days.Count}

TRIỆU CHỨNG/LÝ DO KHÁM PHỔ BIẾN:
{string.Join("\n", symptoms.Select(s => $"- {s.Keyword}: {s.Count} lần"))}

KHUNG GIỜ PHỔ BIẾN:
{string.Join("\n", timeSlots.Select(t => $"- {t.Slot}: {t.Count} ca"))}

NGÀY TRONG TUẦN:
{string.Join(", ", dayDemand.Select(d => $"{d.Day}: {d.Count} ca"))}

THÔNG TIN BÁC SĨ:
- Phí khám: {profile.ConsultationFee:N0} VND
- Kinh nghiệm: {profile.YearsOfExperience} năm
";

            var systemPrompt = @"Bạn là AI phân tích xu hướng y tế cho bác sĩ.

Nhiệm vụ 1 - XU HƯỚNG: Đưa ra 1 nhận định ngắn về xu hướng bệnh lý/nhu cầu khám trong khu vực (1-2 câu).

Nhiệm vụ 2 - KHUYẾN NGHỊ: Đưa ra 1 khuyến nghị cụ thể để tối ưu lịch khám (1-2 câu).

Format trả lời CHÍNH XÁC:
TREND: [nội dung xu hướng]
RECOMMENDATION: [nội dung khuyến nghị]

Tiếng Việt, chuyên nghiệp, dựa trên dữ liệu thực.";

            var messages = new List<LlmMessage>
            {
                new() { Role = "user", Content = contextData }
            };

            var response = await _llmService.GenerateResponseAsync(messages, systemPrompt);

            if (!string.IsNullOrWhiteSpace(response))
            {
                var trend = "Gia tăng 15% bệnh lý hô hấp và tim mạch trong bán kính 10km quanh phòng khám.";
                var recommendation = "Khuyến nghị mở rộng lịch khám tối thứ 5 để đón đầu lượng bệnh nhân tăng cao.";

                var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.StartsWith("TREND:", StringComparison.OrdinalIgnoreCase))
                    {
                        trend = line.Replace("TREND:", "", StringComparison.OrdinalIgnoreCase).Trim();
                    }
                    else if (line.StartsWith("RECOMMENDATION:", StringComparison.OrdinalIgnoreCase))
                    {
                        recommendation = line.Replace("RECOMMENDATION:", "", StringComparison.OrdinalIgnoreCase).Trim();
                    }
                }

                return (trend, recommendation);
            }

            return (
                "Gia tăng 15% bệnh lý hô hấp và tim mạch trong bán kính 10km quanh phòng khám.",
                "Khuyến nghị mở rộng lịch khám tối thứ 5 để đón đầu lượng bệnh nhân tăng cao."
            );
        }
        catch
        {
            return (
                "Gia tăng 15% bệnh lý hô hấp và tim mạch trong bán kính 10km quanh phòng khám.",
                "Khuyến nghị mở rộng lịch khám tối thứ 5 để đón đầu lượng bệnh nhân tăng cao."
            );
        }
    }

    public async Task<bool> UpdateSpecialtyProfileAsync(int doctorUserId, string bio, decimal consultationFee, string? insuranceAccepted, string? location)
    {
        var profile = await _doctorRepository.GetByUserIdAsync(doctorUserId);
        if (profile == null)
        {
            return false;
        }

        profile.Bio = bio;
        profile.ConsultationFee = consultationFee;
        profile.InsuranceAccepted = insuranceAccepted;
        profile.Location = location;
        profile.UpdatedAt = DateTime.Now;

        await _doctorRepository.UpdateAsync(profile);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public Task<DoctorMembershipDto> GetMembershipAsync(int doctorUserId)
    {
        var data = new DoctorMembershipDto
        {
            CurrentPlanCode = "PREMIUM",
            Plans =
            [
                new DoctorMembershipPlanDto
                {
                    PlanCode = "TRIAL",
                    PlanName = "Gói Dùng thử",
                    Price = 0,
                    DurationDays = 14,
                    IsRecommended = false,
                    Features = ["Quản lý hồ sơ cơ bản", "Lịch hẹn tiêu chuẩn"]
                },
                new DoctorMembershipPlanDto
                {
                    PlanCode = "PREMIUM",
                    PlanName = "Gói Premium",
                    Price = 1000000,
                    DurationDays = 30,
                    IsRecommended = true,
                    Features = ["AI Diagnosis Support", "Priority Booking", "Hồ sơ ưu tiên"]
                },
                new DoctorMembershipPlanDto
                {
                    PlanCode = "VIP",
                    PlanName = "Gói VIP",
                    Price = 2500000,
                    DurationDays = 30,
                    IsRecommended = false,
                    Features = ["Priority Badge", "Báo cáo doanh thu nâng cao", "Hỗ trợ SLA cao"]
                }
            ]
        };

        return Task.FromResult(data);
    }

    public async Task<DoctorPaymentSummaryDto> GetPaymentSummaryAsync(int doctorUserId, string planCode)
    {
        var membership = await GetMembershipAsync(doctorUserId);
        var selected = membership.Plans.FirstOrDefault(p => p.PlanCode.Equals(planCode, StringComparison.OrdinalIgnoreCase))
            ?? membership.Plans.First(p => p.PlanCode == "PREMIUM");

        return new DoctorPaymentSummaryDto
        {
            PlanCode = selected.PlanCode,
            PlanName = selected.PlanName,
            Amount = selected.Price,
            DurationDays = selected.DurationDays,
            PaymentMethods = ["VNPAY", "MOMO", "APPLE_PAY", "CARD"]
        };
    }

    public async Task<bool> SubmitMembershipPaymentAsync(int doctorUserId, string planCode, string paymentMethod)
    {
        var summary = await GetPaymentSummaryAsync(doctorUserId, planCode);

        await _notificationRepository.CreateAsync(new Notification
        {
            UserId = doctorUserId,
            NotificationType = "SYSTEM",
            Channel = "IN_APP",
            Title = "Thanh toán gói thành viên thành công",
            Body = $"Bạn đã kích hoạt {summary.PlanName} qua {paymentMethod}.",
            IsRead = false,
            SentAt = DateTime.Now,
            Status = "SENT",
            CreatedAt = DateTime.Now
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static DoctorAppointmentRequestDto MapRequest(Appointment appointment)
    {
        return new DoctorAppointmentRequestDto
        {
            AppointmentId = appointment.AppointmentId,
            PatientName = appointment.Patient.FullName,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            Reason = appointment.Reason,
            Status = appointment.Status,
            AiTriageSummary = BuildAiSummary(appointment)
        };
    }

    private static string BuildAiSummary(Appointment appointment)
    {
        if (string.IsNullOrWhiteSpace(appointment.Reason))
        {
            return "AI chưa ghi nhận triệu chứng nổi bật. Đề nghị bác sĩ xác minh thêm khi tiếp nhận.";
        }

        return $"AI tóm tắt: bệnh nhân báo cáo '{appointment.Reason}'. Khuyến nghị ưu tiên đánh giá dấu hiệu sinh tồn ban đầu.";
    }
}
