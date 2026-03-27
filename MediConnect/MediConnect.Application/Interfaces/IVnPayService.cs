using MediConnect.Application.DTOs;

namespace MediConnect.Application.Interfaces;

public interface IVnPayService
{
    PaymentUrlResultDto CreatePaymentUrl(CreatePaymentRequestDto request);
    PaymentVerifyResultDto VerifyCallback(VnPayCallbackDto callback);
}
