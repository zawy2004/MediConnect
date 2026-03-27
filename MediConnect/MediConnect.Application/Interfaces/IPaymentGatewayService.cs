using MediConnect.Application.DTOs;

namespace MediConnect.Application.Interfaces;

public interface IPaymentGatewayService
{
    Task<PaymentUrlResultDto> CreatePaymentUrlAsync(CreatePaymentRequestDto request);
    Task<PaymentVerifyResultDto> ProcessVnPayCallbackAsync(VnPayCallbackDto callback);
    Task<PaymentVerifyResultDto> ProcessMomoCallbackAsync(MomoCallbackDto callback);
}
