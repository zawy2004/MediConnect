using MediConnect.Application.DTOs;

namespace MediConnect.Application.Interfaces;

public interface IMomoService
{
    Task<PaymentUrlResultDto> CreatePaymentUrlAsync(CreatePaymentRequestDto request);
    PaymentVerifyResultDto VerifyCallback(MomoCallbackDto callback);
}
