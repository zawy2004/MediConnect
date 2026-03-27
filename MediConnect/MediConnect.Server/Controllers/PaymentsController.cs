using MediConnect.Application.DTOs;
using MediConnect.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediConnect.Server.Controllers;

/// <summary>
/// API Controller for payment webhooks (server-to-server callbacks).
/// User-facing payment return is handled by /Patient/PaymentReturn Razor Page.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentGatewayService paymentGatewayService,
        ILogger<PaymentsController> logger)
    {
        _paymentGatewayService = paymentGatewayService;
        _logger = logger;
    }

    /// <summary>
    /// VNPay IPN (Instant Payment Notification) - server-to-server callback
    /// </summary>
    [HttpPost("webhook/vnpay")]
    public async Task<IActionResult> VnPayWebhook()
    {
        var vnpParams = Request.Query;

        var callback = new VnPayCallbackDto
        {
            vnp_TmnCode = vnpParams["vnp_TmnCode"].ToString(),
            vnp_Amount = vnpParams["vnp_Amount"].ToString(),
            vnp_BankCode = vnpParams["vnp_BankCode"].ToString(),
            vnp_BankTranNo = vnpParams["vnp_BankTranNo"].ToString(),
            vnp_CardType = vnpParams["vnp_CardType"].ToString(),
            vnp_PayDate = vnpParams["vnp_PayDate"].ToString(),
            vnp_OrderInfo = vnpParams["vnp_OrderInfo"].ToString(),
            vnp_TransactionNo = vnpParams["vnp_TransactionNo"].ToString(),
            vnp_ResponseCode = vnpParams["vnp_ResponseCode"].ToString(),
            vnp_TransactionStatus = vnpParams["vnp_TransactionStatus"].ToString(),
            vnp_TxnRef = vnpParams["vnp_TxnRef"].ToString(),
            vnp_SecureHash = vnpParams["vnp_SecureHash"].ToString()
        };

        var result = await _paymentGatewayService.ProcessVnPayCallbackAsync(callback);

        _logger.LogInformation("VNPay IPN: OrderId={OrderId}, Success={Success}", result.OrderId, result.Success);

        // VNPay expects specific response format
        return Ok(new { RspCode = result.Success ? "00" : "99", Message = result.Success ? "Confirm Success" : "Confirm Fail" });
    }

    /// <summary>
    /// MoMo IPN (Notify URL) - server-to-server callback
    /// </summary>
    [HttpPost("webhook/momo")]
    public async Task<IActionResult> MomoWebhook([FromBody] MomoCallbackDto callback)
    {
        var result = await _paymentGatewayService.ProcessMomoCallbackAsync(callback);

        _logger.LogInformation("MoMo IPN: OrderId={OrderId}, Success={Success}, TransId={TransId}",
            result.OrderId, result.Success, result.TransactionId);

        // MoMo expects HTTP 204 on success
        return result.Success ? NoContent() : BadRequest();
    }
}
