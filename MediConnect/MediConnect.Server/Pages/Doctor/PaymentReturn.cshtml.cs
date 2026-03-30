using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Doctor;

[AllowAnonymous]
public class PaymentReturnModel : PageModel
{
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly IDoctorPortalService _doctorPortalService;
    private readonly ILogger<PaymentReturnModel> _logger;

    public PaymentReturnModel(
        IPaymentGatewayService paymentGatewayService,
        IDoctorPortalService doctorPortalService,
        ILogger<PaymentReturnModel> logger)
    {
        _paymentGatewayService = paymentGatewayService;
        _doctorPortalService = doctorPortalService;
        _logger = logger;
    }

    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? TransactionId { get; set; }
    public string? OrderId { get; set; }
    public string? PlanCode { get; set; }
    public int DoctorId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (Request.Query.ContainsKey("vnp_ResponseCode"))
        {
            return await HandleVnPayCallbackAsync();
        }

        if (Request.Query.ContainsKey("resultCode"))
        {
            return await HandleMomoCallbackAsync();
        }

        ErrorMessage = "Không xác định được nguồn thanh toán.";
        return Page();
    }

    private async Task<IActionResult> HandleVnPayCallbackAsync()
    {
        var callback = new VnPayCallbackDto
        {
            vnp_TmnCode = Request.Query["vnp_TmnCode"].ToString(),
            vnp_Amount = Request.Query["vnp_Amount"].ToString(),
            vnp_BankCode = Request.Query["vnp_BankCode"].ToString(),
            vnp_BankTranNo = Request.Query["vnp_BankTranNo"].ToString(),
            vnp_CardType = Request.Query["vnp_CardType"].ToString(),
            vnp_PayDate = Request.Query["vnp_PayDate"].ToString(),
            vnp_OrderInfo = Request.Query["vnp_OrderInfo"].ToString(),
            vnp_TransactionNo = Request.Query["vnp_TransactionNo"].ToString(),
            vnp_ResponseCode = Request.Query["vnp_ResponseCode"].ToString(),
            vnp_TransactionStatus = Request.Query["vnp_TransactionStatus"].ToString(),
            vnp_TxnRef = Request.Query["vnp_TxnRef"].ToString(),
            vnp_SecureHash = Request.Query["vnp_SecureHash"].ToString()
        };

        var result = await _paymentGatewayService.ProcessVnPayCallbackAsync(callback);
        _logger.LogInformation("Doctor VNPay callback processed: OrderId={OrderId}, Success={Success}", result.OrderId, result.Success);

        IsSuccess = result.Success;
        TransactionId = result.TransactionId;
        OrderId = result.OrderId;
        ErrorMessage = result.ErrorMessage;

        if (!result.Success)
        {
            return Page();
        }

        var membership = ParseMembershipOrderInfo(callback.vnp_OrderInfo);
        if (membership == null)
        {
            ErrorMessage = "Không xác định được đơn thanh toán gói thành viên.";
            IsSuccess = false;
            return Page();
        }

        DoctorId = membership.DoctorId;
        PlanCode = membership.PlanCode;

        var membershipSuccess = await _doctorPortalService.SubmitMembershipPaymentAsync(membership.DoctorId, membership.PlanCode, "VNPAY");
        if (!membershipSuccess)
        {
            ErrorMessage = "Thanh toán thành công nhưng không thể kích hoạt gói thành viên. Vui lòng thử lại hoặc liên hệ hỗ trợ.";
            IsSuccess = false;
        }

        return Page();
    }

    private async Task<IActionResult> HandleMomoCallbackAsync()
    {
        var callback = new MomoCallbackDto
        {
            PartnerCode = Request.Query["partnerCode"].ToString(),
            OrderId = Request.Query["orderId"].ToString(),
            RequestId = Request.Query["requestId"].ToString(),
            Amount = long.TryParse(Request.Query["amount"], out var amount) ? amount : 0,
            OrderInfo = Request.Query["orderInfo"].ToString(),
            OrderType = Request.Query["orderType"].ToString(),
            TransId = long.TryParse(Request.Query["transId"], out var transId) ? transId : 0,
            ResultCode = int.TryParse(Request.Query["resultCode"], out var resultCode) ? resultCode : -1,
            Message = Request.Query["message"].ToString(),
            PayType = Request.Query["payType"].ToString(),
            ResponseTime = long.TryParse(Request.Query["responseTime"], out var responseTime) ? responseTime : 0,
            ExtraData = Request.Query["extraData"].ToString(),
            Signature = Request.Query["signature"].ToString()
        };

        var result = await _paymentGatewayService.ProcessMomoCallbackAsync(callback);
        _logger.LogInformation("Doctor MoMo callback processed: OrderId={OrderId}, Success={Success}", result.OrderId, result.Success);

        IsSuccess = result.Success;
        TransactionId = result.TransactionId;
        OrderId = result.OrderId;
        ErrorMessage = result.ErrorMessage;

        if (!result.Success)
        {
            return Page();
        }

        var membership = ParseMembershipOrderInfo(callback.OrderInfo);
        if (membership == null)
        {
            ErrorMessage = "Không xác định được đơn thanh toán gói thành viên.";
            IsSuccess = false;
            return Page();
        }

        DoctorId = membership.DoctorId;
        PlanCode = membership.PlanCode;

        var membershipSuccess = await _doctorPortalService.SubmitMembershipPaymentAsync(membership.DoctorId, membership.PlanCode, "MOMO");
        if (!membershipSuccess)
        {
            ErrorMessage = "Thanh toán thành công nhưng không thể kích hoạt gói thành viên. Vui lòng thử lại hoặc liên hệ hỗ trợ.";
            IsSuccess = false;
        }

        return Page();
    }

    private static MembershipOrderInfo? ParseMembershipOrderInfo(string orderInfo)
    {
        if (string.IsNullOrWhiteSpace(orderInfo))
        {
            return null;
        }

        var parts = orderInfo.Split('|', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3 || !parts[0].Equals("DOCTOR_MEMBERSHIP", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var doctorId = 0;
        var planCode = string.Empty;

        foreach (var part in parts.Skip(1))
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2)
            {
                continue;
            }

            if (kv[0].Equals("doctorId", StringComparison.OrdinalIgnoreCase) && int.TryParse(kv[1], out var parsedDoctorId))
            {
                doctorId = parsedDoctorId;
            }
            else if (kv[0].Equals("planCode", StringComparison.OrdinalIgnoreCase))
            {
                planCode = kv[1];
            }
        }

        if (doctorId <= 0 || string.IsNullOrWhiteSpace(planCode))
        {
            return null;
        }

        return new MembershipOrderInfo(doctorId, planCode);
    }

    private sealed record MembershipOrderInfo(int DoctorId, string PlanCode);
}
