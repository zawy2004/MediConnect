using System.Net;
using System.Security.Cryptography;
using System.Text;
using MediConnect.Application.Configurations;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace MediConnect.Application.Services;

public class VnPayService : IVnPayService
{
    private readonly VnPaySettings _settings;

    public VnPayService(IOptions<VnPaySettings> settings)
    {
        _settings = settings.Value;
    }

    public PaymentUrlResultDto CreatePaymentUrl(CreatePaymentRequestDto request)
    {
        var orderId = $"MC{request.AppointmentId:D6}{DateTime.Now:HHmmss}";
        var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");
        var expireDate = DateTime.Now.AddMinutes(_settings.ExpireMinutes).ToString("yyyyMMddHHmmss");

        var vnpParams = new SortedDictionary<string, string>
        {
            { "vnp_Version", _settings.Version },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", _settings.TmnCode },
            { "vnp_Amount", ((long)(request.Amount * 100)).ToString() },
            { "vnp_CurrCode", request.Currency },
            { "vnp_TxnRef", orderId },
            { "vnp_OrderInfo", WebUtility.UrlEncode(request.OrderInfo) },
            { "vnp_OrderType", _settings.OrderType },
            { "vnp_Locale", _settings.Locale },
            { "vnp_ReturnUrl", _settings.ReturnUrl },
            { "vnp_IpAddr", request.ClientIpAddress },
            { "vnp_CreateDate", createDate },
            { "vnp_ExpireDate", expireDate }
        };

        var queryString = BuildQueryString(vnpParams);
        var signData = queryString;
        var secureHash = HmacSha512(_settings.HashSecret, signData);

        var paymentUrl = $"{_settings.BaseUrl}?{queryString}&vnp_SecureHash={secureHash}";

        return new PaymentUrlResultDto
        {
            Success = true,
            PaymentUrl = paymentUrl,
            OrderId = orderId
        };
    }

    public PaymentVerifyResultDto VerifyCallback(VnPayCallbackDto callback)
    {
        var vnpParams = new SortedDictionary<string, string>
        {
            { "vnp_TmnCode", callback.vnp_TmnCode },
            { "vnp_Amount", callback.vnp_Amount },
            { "vnp_BankCode", callback.vnp_BankCode },
            { "vnp_BankTranNo", callback.vnp_BankTranNo },
            { "vnp_CardType", callback.vnp_CardType },
            { "vnp_PayDate", callback.vnp_PayDate },
            { "vnp_OrderInfo", callback.vnp_OrderInfo },
            { "vnp_TransactionNo", callback.vnp_TransactionNo },
            { "vnp_ResponseCode", callback.vnp_ResponseCode },
            { "vnp_TransactionStatus", callback.vnp_TransactionStatus },
            { "vnp_TxnRef", callback.vnp_TxnRef }
        };

        // Remove empty values
        var filteredParams = vnpParams.Where(p => !string.IsNullOrEmpty(p.Value))
            .ToDictionary(p => p.Key, p => p.Value);
        var sortedParams = new SortedDictionary<string, string>(filteredParams);

        var signData = BuildQueryString(sortedParams);
        var computedHash = HmacSha512(_settings.HashSecret, signData);

        if (!computedHash.Equals(callback.vnp_SecureHash, StringComparison.OrdinalIgnoreCase))
        {
            return new PaymentVerifyResultDto
            {
                Success = false,
                ErrorMessage = "Invalid signature"
            };
        }

        var isSuccess = callback.vnp_ResponseCode == "00" && callback.vnp_TransactionStatus == "00";

        return new PaymentVerifyResultDto
        {
            Success = isSuccess,
            TransactionId = callback.vnp_TransactionNo,
            OrderId = callback.vnp_TxnRef,
            Amount = decimal.Parse(callback.vnp_Amount) / 100,
            ResponseCode = callback.vnp_ResponseCode,
            ErrorMessage = isSuccess ? null : GetErrorMessage(callback.vnp_ResponseCode)
        };
    }

    private static string BuildQueryString(SortedDictionary<string, string> data)
    {
        var query = new StringBuilder();
        foreach (var kv in data)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                if (query.Length > 0) query.Append('&');
                query.Append($"{kv.Key}={kv.Value}");
            }
        }
        return query.ToString();
    }

    private static string HmacSha512(string key, string data)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    private static string GetErrorMessage(string responseCode)
    {
        return responseCode switch
        {
            "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
            "09" => "Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
            "10" => "Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần.",
            "11" => "Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch.",
            "12" => "Thẻ/Tài khoản của khách hàng bị khóa.",
            "13" => "Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
            "24" => "Khách hàng hủy giao dịch.",
            "51" => "Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
            "65" => "Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
            "75" => "Ngân hàng thanh toán đang bảo trì.",
            "79" => "KH nhập sai mật khẩu thanh toán quá số lần quy định.",
            "99" => "Các lỗi khác.",
            _ => $"Lỗi không xác định: {responseCode}"
        };
    }
}
