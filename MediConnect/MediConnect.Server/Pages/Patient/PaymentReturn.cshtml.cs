using System.Linq;
using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MediConnect.Server.Pages.Patient;

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
    public int AppointmentId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Check if this is VNPay callback
        if (Request.Query.ContainsKey("vnp_ResponseCode"))
        {
            return await HandleVnPayCallbackAsync();
        }

        // Check if this is MoMo callback
        if (Request.Query.ContainsKey("resultCode"))
        {
            return await HandleMomoCallbackAsync();
        }

        // Unknown callback
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

        _logger.LogInformation("VNPay return processed: OrderId={OrderId}, Success={Success}",
            result.OrderId, result.Success);

        IsSuccess = result.Success;
        TransactionId = result.TransactionId;
        OrderId = result.OrderId;
        ErrorMessage = result.ErrorMessage;

        AppointmentId = ExtractAppointmentId(result.OrderId);

        if (IsSuccess)
        {
            // Parse OrderInfo để lấy thông tin đặt lịch
            var orderInfo = callback.vnp_OrderInfo;
            var appointmentDto = ParseOrderInfo(orderInfo, User);
            if (appointmentDto == null)
            {
                ErrorMessage = "Không thể lấy thông tin đặt lịch từ thanh toán.";
                return Page();
            }
            var appointmentService = HttpContext.RequestServices.GetService(typeof(MediConnect.Application.Interfaces.IAppointmentService)) as MediConnect.Application.Interfaces.IAppointmentService;
            if (appointmentService == null)
            {
                ErrorMessage = "Không thể khởi tạo dịch vụ lịch hẹn.";
                return Page();
            }
            var createResult = await appointmentService.CreateAppointmentAsync(appointmentDto);
            if (!createResult.Success || !createResult.AppointmentId.HasValue)
            {
                ErrorMessage = createResult.ErrorMessage ?? "Không thể tạo lịch hẹn sau thanh toán.";
                return Page();
            }
            AppointmentId = createResult.AppointmentId.Value;
            return RedirectToPage("/Patient/BookingSuccess", new { appointmentId = AppointmentId, transactionId = TransactionId });
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

        _logger.LogInformation("MoMo return processed: OrderId={OrderId}, Success={Success}",
            result.OrderId, result.Success);

        IsSuccess = result.Success;
        TransactionId = result.TransactionId;
        OrderId = result.OrderId;
        ErrorMessage = result.ErrorMessage;

        AppointmentId = ExtractAppointmentId(result.OrderId);

        if (IsSuccess)
        {
            // Parse OrderInfo để lấy thông tin đặt lịch
            var orderInfo = callback.OrderInfo;
            var appointmentDto = ParseOrderInfo(orderInfo, User);
            if (appointmentDto == null)
            {
                ErrorMessage = "Không thể lấy thông tin đặt lịch từ thanh toán.";
                return Page();
            }
            var appointmentService = HttpContext.RequestServices.GetService(typeof(MediConnect.Application.Interfaces.IAppointmentService)) as MediConnect.Application.Interfaces.IAppointmentService;
            if (appointmentService == null)
            {
                ErrorMessage = "Không thể khởi tạo dịch vụ lịch hẹn.";
                return Page();
            }
            var createResult = await appointmentService.CreateAppointmentAsync(appointmentDto);
            if (!createResult.Success || !createResult.AppointmentId.HasValue)
            {
                ErrorMessage = createResult.ErrorMessage ?? "Không thể tạo lịch hẹn sau thanh toán.";
                return Page();
            }
            AppointmentId = createResult.AppointmentId.Value;
            return RedirectToPage("/Patient/BookingSuccess", new { appointmentId = AppointmentId, transactionId = TransactionId });
        }

        return Page();
    }

    // Parse OrderInfo string to CreateAppointmentDto
    private MediConnect.Application.DTOs.CreateAppointmentDto? ParseOrderInfo(string? orderInfo, System.Security.Claims.ClaimsPrincipal user)
    {
        if (string.IsNullOrEmpty(orderInfo)) return null;
        var dict = new Dictionary<string, string>();
        foreach (var part in orderInfo.Split(';'))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2)
                dict[kv[0]] = kv[1];
        }
        try
        {
            var patientId = int.Parse(user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var doctorUserId = int.Parse(dict["doctorUserId"]);
            var slotId = int.Parse(dict["slotId"]);
            int? specialtyId = null;
            if (dict.ContainsKey("specialtyId") && int.TryParse(dict["specialtyId"], out var spId)) specialtyId = spId;
            var appointmentDate = DateTime.Parse(dict["appointmentDate"]);
            var reason = dict.ContainsKey("reason") ? Uri.UnescapeDataString(dict["reason"]) : null;
            return new MediConnect.Application.DTOs.CreateAppointmentDto
            {
                PatientId = patientId,
                DoctorId = doctorUserId,
                SlotId = slotId,
                SpecialtyId = specialtyId,
                AppointmentDate = appointmentDate,
                Reason = reason
            };
        }
        catch
        {
            return null;
        }
    }

    private static int ExtractAppointmentId(string orderId)
    {
        // OrderId format: MC{appointmentId:D6}{timestamp}
        if (!string.IsNullOrEmpty(orderId) && orderId.StartsWith("MC") && orderId.Length >= 8)
        {
            var appointmentIdStr = orderId.Substring(2, 6);
            if (int.TryParse(appointmentIdStr, out var appointmentId))
            {
                return appointmentId;
            }
        }
        return 0;
    }
}
