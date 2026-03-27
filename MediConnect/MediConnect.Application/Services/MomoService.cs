using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediConnect.Application.Configurations;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace MediConnect.Application.Services;

public class MomoService : IMomoService
{
    private readonly MomoSettings _settings;
    private readonly HttpClient _httpClient;

    public MomoService(IOptions<MomoSettings> settings, HttpClient httpClient)
    {
        _settings = settings.Value;
        _httpClient = httpClient;
    }

    public async Task<PaymentUrlResultDto> CreatePaymentUrlAsync(CreatePaymentRequestDto request)
    {
        var orderId = $"MC{request.AppointmentId:D6}{DateTime.Now:HHmmss}";
        var requestId = Guid.NewGuid().ToString();
        var amount = (long)request.Amount;
        var extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes($"appointmentId={request.AppointmentId}"));

        var rawSignature = $"accessKey={_settings.AccessKey}" +
                          $"&amount={amount}" +
                          $"&extraData={extraData}" +
                          $"&ipnUrl={_settings.NotifyUrl}" +
                          $"&orderId={orderId}" +
                          $"&orderInfo={request.OrderInfo}" +
                          $"&partnerCode={_settings.PartnerCode}" +
                          $"&redirectUrl={_settings.ReturnUrl}" +
                          $"&requestId={requestId}" +
                          $"&requestType={_settings.RequestType}";

        var signature = HmacSha256(_settings.SecretKey, rawSignature);

        var requestBody = new
        {
            partnerCode = _settings.PartnerCode,
            partnerName = "MediConnect",
            storeId = "MediConnectStore",
            requestId,
            amount,
            orderId,
            orderInfo = request.OrderInfo,
            redirectUrl = _settings.ReturnUrl,
            ipnUrl = _settings.NotifyUrl,
            lang = "vi",
            requestType = _settings.RequestType,
            autoCapture = true,
            extraData,
            signature
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(_settings.MomoApiUrl, requestBody);
            var content = await response.Content.ReadAsStringAsync();

            // MoMo trả JSON theo camelCase (ex: resultCode, payUrl) nên cần case-insensitive
            var result = JsonSerializer.Deserialize<MomoCreatePaymentResponse>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!response.IsSuccessStatusCode)
            {
                return new PaymentUrlResultDto
                {
                    Success = false,
                    ErrorMessage = $"MoMo API trả lỗi HTTP {(int)response.StatusCode}: {content}",
                    OrderId = orderId
                };
            }

            var payUrl = result?.PayUrl;
            if (result?.ResultCode == 0 && !string.IsNullOrWhiteSpace(payUrl))
            {
                return new PaymentUrlResultDto
                {
                    Success = true,
                    PaymentUrl = payUrl,
                    OrderId = orderId
                };
            }

            var message = result?.Message;
            return new PaymentUrlResultDto
            {
                Success = false,
                ErrorMessage = string.IsNullOrWhiteSpace(message) ? "Không thể tạo link thanh toán MoMo" : message,
                OrderId = orderId
            };
        }
        catch (Exception ex)
        {
            return new PaymentUrlResultDto
            {
                Success = false,
                ErrorMessage = $"Lỗi kết nối MoMo: {ex.Message}",
                OrderId = orderId
            };
        }
    }

    public PaymentVerifyResultDto VerifyCallback(MomoCallbackDto callback)
    {
        var rawSignature = $"accessKey={_settings.AccessKey}" +
                          $"&amount={callback.Amount}" +
                          $"&extraData={callback.ExtraData}" +
                          $"&message={callback.Message}" +
                          $"&orderId={callback.OrderId}" +
                          $"&orderInfo={callback.OrderInfo}" +
                          $"&orderType={callback.OrderType}" +
                          $"&partnerCode={callback.PartnerCode}" +
                          $"&payType={callback.PayType}" +
                          $"&requestId={callback.RequestId}" +
                          $"&responseTime={callback.ResponseTime}" +
                          $"&resultCode={callback.ResultCode}" +
                          $"&transId={callback.TransId}";

        var computedSignature = HmacSha256(_settings.SecretKey, rawSignature);

        if (!computedSignature.Equals(callback.Signature, StringComparison.OrdinalIgnoreCase))
        {
            return new PaymentVerifyResultDto
            {
                Success = false,
                ErrorMessage = "Invalid signature"
            };
        }

        var isSuccess = callback.ResultCode == 0;

        return new PaymentVerifyResultDto
        {
            Success = isSuccess,
            TransactionId = callback.TransId.ToString(),
            OrderId = callback.OrderId,
            Amount = callback.Amount,
            ResponseCode = callback.ResultCode.ToString(),
            ErrorMessage = isSuccess ? null : callback.Message
        };
    }

    private static string HmacSha256(string key, string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    private class MomoCreatePaymentResponse
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public long ResponseTime { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ResultCode { get; set; }
        public string PayUrl { get; set; } = string.Empty;
        public string DeepLink { get; set; } = string.Empty;
        public string QrCodeUrl { get; set; } = string.Empty;
    }
}
