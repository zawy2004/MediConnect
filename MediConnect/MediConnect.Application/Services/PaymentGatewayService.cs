using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using MediConnect.Domain.Entities;

namespace MediConnect.Application.Services;

public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly IVnPayService _vnPayService;
    private readonly IMomoService _momoService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentGatewayService(
        IVnPayService vnPayService,
        IMomoService momoService,
        IPaymentRepository paymentRepository,
        IAppointmentRepository appointmentRepository,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _vnPayService = vnPayService;
        _momoService = momoService;
        _paymentRepository = paymentRepository;
        _appointmentRepository = appointmentRepository;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentUrlResultDto> CreatePaymentUrlAsync(CreatePaymentRequestDto request)
    {
        // Create pending payment record
        var normalizedAppointmentId = request.AppointmentId.HasValue && request.AppointmentId.Value > 0
            ? request.AppointmentId.Value
            : (int?)null;

        var payment = new Payment
        {
            AppointmentId = normalizedAppointmentId,
            PatientId = request.PatientId,
            Amount = request.Amount,
            Currency = request.Currency,
            PaymentMethod = request.PaymentMethod,
            PaymentStatus = "PENDING",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _paymentRepository.CreateAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        // Generate payment URL based on method
        PaymentUrlResultDto result;

        switch (request.PaymentMethod.ToUpperInvariant())
        {
            case "VNPAY":
                result = _vnPayService.CreatePaymentUrl(request);
                break;
            case "MOMO":
                result = await _momoService.CreatePaymentUrlAsync(request);
                break;
            default:
                return new PaymentUrlResultDto
                {
                    Success = false,
                    ErrorMessage = $"Phương thức thanh toán không được hỗ trợ: {request.PaymentMethod}"
                };
        }

        return result;
    }

    public async Task<PaymentVerifyResultDto> ProcessVnPayCallbackAsync(VnPayCallbackDto callback)
    {
        var result = _vnPayService.VerifyCallback(callback);

        if (result.Success)
        {
            await UpdatePaymentStatusAsync(result.OrderId, result.TransactionId, "PAID");
        }

        return result;
    }

    public async Task<PaymentVerifyResultDto> ProcessMomoCallbackAsync(MomoCallbackDto callback)
    {
        var result = _momoService.VerifyCallback(callback);

        if (result.Success)
        {
            await UpdatePaymentStatusAsync(result.OrderId, result.TransactionId, "PAID");
        }

        return result;
    }

    private async Task UpdatePaymentStatusAsync(string orderId, string transactionId, string status)
    {
        // Extract appointmentId from orderId (MC{appointmentId:D6}{timestamp})
        if (orderId.StartsWith("MC") && orderId.Length >= 8)
        {
            var appointmentIdStr = orderId.Substring(2, 6);
            if (int.TryParse(appointmentIdStr, out var appointmentId))
            {
                var payment = await _paymentRepository.GetLatestByAppointmentAsync(appointmentId);
                if (payment != null)
                {
                    var wasPaid = string.Equals(payment.PaymentStatus, "PAID", StringComparison.OrdinalIgnoreCase);

                    payment.PaymentStatus = status;
                    payment.TransactionId = transactionId;
                    payment.PaidAt = DateTime.Now;
                    payment.UpdatedAt = DateTime.Now;

                    // Create notification when the payment transitions to PAID.
                    if (!wasPaid && string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase))
                    {
                        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
                        if (appointment != null)
                        {
                            await _notificationRepository.CreateAsync(new Notification
                            {
                                UserId = appointment.PatientId,
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
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                }
            }
        }
    }
}
