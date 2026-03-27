using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Constants;
using MediConnect.Domain.Entities;

namespace MediConnect.Application.Services;

public class PatientPortalService : IPatientPortalService
{
    private readonly IUserRepository _userRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IDoctorService _doctorService;
    private readonly ITimeSlotRepository _timeSlotRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRagService? _ragService;

    public PatientPortalService(
        IUserRepository userRepository,
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IDoctorService doctorService,
        ITimeSlotRepository timeSlotRepository,
        IPaymentRepository paymentRepository,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        IRagService? ragService = null)
    {
        _userRepository = userRepository;
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _doctorService = doctorService;
        _timeSlotRepository = timeSlotRepository;
        _paymentRepository = paymentRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _ragService = ragService;
    }

    public async Task<PatientPortalDashboardDto> GetDashboardAsync(int patientId)
    {
        var user = await _userRepository.GetByIdAsync(patientId);
        var appointments = await _appointmentRepository.GetByPatientIdAsync(patientId);

        return new PatientPortalDashboardDto
        {
            PatientName = user?.FullName ?? "Bệnh nhân",
            UpcomingAppointments = appointments.Count(a => a.AppointmentDate >= DateOnly.FromDateTime(DateTime.Today) && a.Status != AppointmentStatus.CancelledByPatient && a.Status != AppointmentStatus.CancelledByDoctor),
            CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
            CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.CancelledByDoctor || a.Status == AppointmentStatus.CancelledByPatient),
            Schedule = appointments
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Take(12)
                .Select(MapToAppointmentList)
                .ToList(),
            Insights = new List<string>
            {
                "Giữ nhịp tái khám đúng hẹn để bác sĩ theo dõi tiến triển điều trị.",
                "Sử dụng AI Symptom Assessment trước khi đặt lịch để tăng độ chính xác chuyên khoa.",
                "Ưu tiên khung giờ sáng nếu bạn cần giảm thời gian chờ tại phòng khám."
            }
        };
    }

    public async Task<PatientDoctorScheduleDto?> GetDoctorScheduleAsync(int doctorUserId, DateOnly fromDate, int days)
    {
        var profile = await _doctorRepository.GetByUserIdAsync(doctorUserId);
        if (profile == null) return null;

        var toDate = fromDate.AddDays(Math.Max(1, days) - 1);
        var slots = await _timeSlotRepository.GetByDoctorAndDateRangeAsync(doctorUserId, fromDate, toDate);

        return new PatientDoctorScheduleDto
        {
            DoctorUserId = doctorUserId,
            DoctorName = profile.User.FullName,
            FromDate = fromDate,
            ToDate = toDate,
            Slots = slots.Select(s => new PatientDoctorSlotDto
            {
                SlotId = s.SlotId,
                SlotDate = s.SlotDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsAvailable = s.IsAvailable && s.BookedCount < s.MaxCapacity,
                StatusLabel = s.IsAvailable && s.BookedCount < s.MaxCapacity ? "Available" : "Unavailable"
            }).ToList()
        };
    }

    public async Task<PatientDoctorProfileDto?> GetDoctorProfileAsync(int doctorProfileId)
    {
        var detail = await _doctorService.GetDoctorDetailAsync(doctorProfileId);
        if (detail == null) return null;

        var fromDate = DateOnly.FromDateTime(DateTime.Today);
        var slots = await _timeSlotRepository.GetByDoctorAndDateRangeAsync(detail.UserId, fromDate, fromDate.AddDays(14));

        return new PatientDoctorProfileDto
        {
            Doctor = detail,
            NextSlots = slots.Where(s => s.IsAvailable && s.BookedCount < s.MaxCapacity)
                .OrderBy(s => s.SlotDate)
                .ThenBy(s => s.StartTime)
                .Take(12)
                .Select(s => new PatientDoctorSlotDto
                {
                    SlotId = s.SlotId,
                    SlotDate = s.SlotDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    IsAvailable = true,
                    StatusLabel = "Available"
                }).ToList(),
            TotalVisits = 1200,
            SatisfiedPercent = 98,
            AwardsCount = 25
        };
    }

    public async Task<PatientAppointmentManagerDto> GetAppointmentManagerAsync(int patientId)
    {
        var appointments = await _appointmentRepository.GetByPatientIdAsync(patientId);

        return new PatientAppointmentManagerDto
        {
            TotalAppointments = appointments.Count,
            PendingAppointments = appointments.Count(a => a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed),
            CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
            CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.CancelledByDoctor || a.Status == AppointmentStatus.CancelledByPatient),
            Appointments = appointments.OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime).Select(MapToAppointmentList).ToList()
        };
    }

    public async Task<PatientTriageResultDto> AnalyzeSymptomsAsync(int patientId, string symptomText)
    {
        string suggestedSpecialty;
        int riskScore;
        string reply;

        // Try RAG-based analysis first
        if (_ragService != null)
        {
            try
            {
                var ragResult = await _ragService.AnalyzeSymptomsAsync(symptomText);
                suggestedSpecialty = ragResult.SuggestedSpecialty;
                riskScore = ragResult.RiskScore;
                reply = ragResult.AssistantReply;
            }
            catch
            {
                // Fallback to basic analysis
                (suggestedSpecialty, riskScore, reply) = FallbackSymptomAnalysis(symptomText);
            }
        }
        else
        {
            // No RAG service, use fallback
            (suggestedSpecialty, riskScore, reply) = FallbackSymptomAnalysis(symptomText);
        }

        var doctors = await _doctorService.SearchDoctorsAsync(new DoctorSearchFilterDto { SearchTerm = suggestedSpecialty == "Nội tim mạch" ? "tim" : null });

        return new PatientTriageResultDto
        {
            SymptomText = symptomText,
            AssistantReply = reply,
            SuggestedSpecialty = suggestedSpecialty,
            RiskScore = riskScore,
            SuggestedDoctors = doctors.Take(4).ToList()
        };
    }

    private static (string specialty, int risk, string reply) FallbackSymptomAnalysis(string symptomText)
    {
        var lowered = symptomText.ToLowerInvariant();
        var suggestedSpecialty = "Tổng quát";
        var riskScore = 30;
        var reply = "Mức độ thấp. Bạn nên đặt lịch tư vấn tổng quát để bác sĩ đánh giá trực tiếp.";

        if (lowered.Contains("tức ngực") || lowered.Contains("khó thở") || lowered.Contains("đau tim"))
        {
            suggestedSpecialty = "Nội tim mạch";
            riskScore = 85;
            reply = "Triệu chứng có nguy cơ cao. Bạn nên khám Nội tim mạch trong ngày và theo dõi dấu hiệu bất thường.";
        }
        else if (lowered.Contains("ho") || lowered.Contains("sốt") || lowered.Contains("viêm") || lowered.Contains("mệt"))
        {
            suggestedSpecialty = "Nội tổng quát";
            riskScore = 55;
            reply = "Mức độ trung bình. Bạn nên đặt lịch khám trong 24 giờ để được kiểm tra chuyên sâu.";
        }

        return (suggestedSpecialty, riskScore, reply);
    }

    public async Task<PatientPaymentConfirmDto?> GetPaymentConfirmAsync(int patientId, int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null || appointment.PatientId != patientId)
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(patientId);
        var amount = appointment.Doctor.DoctorProfileUser?.ConsultationFee ?? 0m;

        return new PatientPaymentConfirmDto
        {
            AppointmentId = appointmentId,
            PatientName = user?.FullName ?? appointment.Patient.FullName,
            PhoneNumber = user?.PhoneNumber,
            DoctorName = appointment.Doctor.FullName,
            SpecialtyName = appointment.Specialty?.SpecialtyName,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            Location = appointment.Doctor.DoctorProfileUser?.Location,
            Amount = amount > 0 ? amount : 250000m
        };
    }

    public async Task<PatientPaymentResultDto> CompletePaymentAsync(int patientId, int appointmentId, string paymentMethod)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null || appointment.PatientId != patientId)
        {
            return new PatientPaymentResultDto { Success = false, ErrorMessage = "Không tìm thấy lịch hẹn để thanh toán." };
        }

        var existing = await _paymentRepository.GetLatestByAppointmentAsync(appointmentId);
        if (existing != null && existing.PaymentStatus == "PAID")
        {
            return new PatientPaymentResultDto
            {
                Success = true,
                PaymentId = existing.PaymentId,
                TransactionId = existing.TransactionId ?? string.Empty
            };
        }

        var transactionId = $"MC-{DateTime.UtcNow:yyyyMMddHHmmss}-{appointmentId}";
        var amount = appointment.Doctor.DoctorProfileUser?.ConsultationFee ?? 250000m;

        var payment = new Payment
        {
            AppointmentId = appointmentId,
            PatientId = patientId,
            Amount = amount,
            Currency = "VND",
            PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "VNPAY" : paymentMethod,
            PaymentStatus = "PAID",
            TransactionId = transactionId,
            PaidAt = DateTime.Now,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _paymentRepository.CreateAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // Create in-app notification on successful (onsite/CASH) payment.
        await _notificationRepository.CreateAsync(new Notification
        {
            UserId = patientId,
            AppointmentId = appointment.AppointmentId,
            NotificationType = "BOOKING_CONFIRMATION",
            Channel = "IN_APP",
            Title = "Đặt lịch thành công",
            Body =
                $"Bạn đã thanh toán thành công cho lịch hẹn ngày {appointment.AppointmentDate:dd/MM/yyyy} lúc {appointment.StartTime:HH:mm} với bác sĩ {appointment.Doctor?.FullName}.",
            IsRead = false,
            SentAt = DateTime.Now,
            Status = "SENT",
            CreatedAt = DateTime.Now
        });
        await _unitOfWork.SaveChangesAsync();

        return new PatientPaymentResultDto
        {
            Success = true,
            PaymentId = payment.PaymentId,
            TransactionId = transactionId
        };
    }

    public async Task<PatientBookingSuccessDto?> GetBookingSuccessAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null) return null;

        var payment = await _paymentRepository.GetLatestByAppointmentAsync(appointmentId);

        return new PatientBookingSuccessDto
        {
            AppointmentId = appointmentId,
            BookingCode = $"MC-{appointmentId:0000}-{appointment.AppointmentDate:ddMM}",
            DoctorName = appointment.Doctor.FullName,
            SpecialtyName = appointment.Specialty?.SpecialtyName,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            Location = appointment.Doctor.DoctorProfileUser?.Location,
            Status = appointment.Status,
            TransactionId = payment?.TransactionId
        };
    }

    public async Task<List<PatientNotificationItemDto>> GetRecentInAppNotificationsAsync(int patientId, int take = 20)
    {
        var safeTake = Math.Clamp(take, 1, 50);
        var notifications = await _notificationRepository.GetRecentByUserIdAndChannelAsync(patientId, "IN_APP", safeTake);
        return notifications.Select(n => new PatientNotificationItemDto
        {
            NotificationId = n.NotificationId,
            AppointmentId = n.AppointmentId,
            NotificationType = n.NotificationType,
            Channel = n.Channel,
            Title = n.Title,
            Body = n.Body,
            IsRead = n.IsRead,
            Status = n.Status,
            CreatedAt = n.CreatedAt,
            SentAt = n.SentAt
        }).ToList();
    }

    public async Task<List<PatientPaymentHistoryItemDto>> GetPaidAppointmentPaymentHistoryAsync(int patientId, int take = 20)
    {
        var safeTake = Math.Clamp(take, 1, 50);

        // Load appointments once to avoid N+1 queries.
        var appointments = await _appointmentRepository.GetByPatientIdAsync(patientId);
        var appointmentMap = appointments.ToDictionary(a => a.AppointmentId);

        var payments = await _paymentRepository.GetByPatientIdAsync(patientId);

        var paidSuccessful = payments
            .Where(p => string.Equals(p.PaymentStatus, "PAID", StringComparison.OrdinalIgnoreCase))
            .Where(p => appointmentMap.TryGetValue(p.AppointmentId, out var appt)
                        && appt.Status != AppointmentStatus.CancelledByPatient
                        && appt.Status != AppointmentStatus.CancelledByDoctor)
            .OrderByDescending(p => p.PaidAt ?? p.CreatedAt)
            .Take(safeTake);

        return paidSuccessful
            .Select(p =>
            {
                appointmentMap.TryGetValue(p.AppointmentId, out var appt);
                return new PatientPaymentHistoryItemDto
                {
                    PaymentId = p.PaymentId,
                    AppointmentId = p.AppointmentId,
                    BookingCode = appt != null
                        ? $"MC-{appt.AppointmentId:0000}-{appt.AppointmentDate:ddMM}"
                        : $"MC-{p.AppointmentId:0000}",
                    DoctorName = appt?.Doctor?.FullName ?? "Bác sĩ",
                    SpecialtyName = appt?.Specialty?.SpecialtyName,
                    AppointmentDate = appt?.AppointmentDate ?? DateOnly.FromDateTime(p.CreatedAt),
                    StartTime = appt?.StartTime ?? TimeOnly.FromDateTime(p.CreatedAt),
                    Amount = p.Amount,
                    Currency = p.Currency,
                    PaymentMethod = p.PaymentMethod,
                    TransactionId = p.TransactionId,
                    PaidAt = p.PaidAt
                };
            })
            .ToList();
    }

    public async Task<PatientProfilePortalDto?> GetProfileAsync(int patientId)
    {
        var user = await _userRepository.GetByIdAsync(patientId);
        if (user == null) return null;

        return new PatientProfilePortalDto
        {
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Address = user.Address,
            BloodType = "O+",
            Bmi = 22.4m,
            WeightTrend = new()
            {
                new() { Label = "T-5", Value = 66.2m },
                new() { Label = "T-4", Value = 66.0m },
                new() { Label = "T-3", Value = 65.8m },
                new() { Label = "T-2", Value = 65.6m },
                new() { Label = "T-1", Value = 65.7m },
                new() { Label = "Hiện tại", Value = 65.5m }
            },
            BloodPressureTrend = new()
            {
                new() { Label = "T-5", Value = 128 },
                new() { Label = "T-4", Value = 126 },
                new() { Label = "T-3", Value = 124 },
                new() { Label = "T-2", Value = 122 },
                new() { Label = "T-1", Value = 121 },
                new() { Label = "Hiện tại", Value = 120 }
            },
            Alerts = new()
            {
                "Thời tiết giao mùa: ưu tiên giữ ấm và theo dõi hô hấp vào buổi sáng.",
                "Mức vận động tuần này thấp hơn khuyến nghị 12% - nên bổ sung 2 buổi đi bộ nhẹ."
            }
        };
    }

    public async Task<bool> UpdateProfileAsync(int patientId, string fullName, string? phoneNumber, string? gender, DateOnly? dateOfBirth, string? address)
    {
        var user = await _userRepository.GetByIdAsync(patientId);
        if (user == null) return false;

        user.FullName = fullName;
        user.PhoneNumber = phoneNumber;
        user.Gender = gender;
        user.DateOfBirth = dateOfBirth;
        user.Address = address;
        user.UpdatedAt = DateTime.Now;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static AppointmentListDto MapToAppointmentList(Appointment appointment)
    {
        return new AppointmentListDto
        {
            AppointmentId = appointment.AppointmentId,
            PatientName = appointment.Patient.FullName,
            DoctorName = appointment.Doctor.FullName,
            SpecialtyName = appointment.Specialty?.SpecialtyName,
            AppointmentDate = appointment.AppointmentDate,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            Reason = appointment.Reason,
            Status = appointment.Status,
            CreatedAt = appointment.CreatedAt
        };
    }
}
