namespace MediConnect.Application.Configurations;

public class VnPaySettings
{
    public string BaseUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string IpnUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string DoctorReturnUrl { get; set; } = string.Empty;
    public string Locale { get; set; } = "vn";
    public string Version { get; set; } = "2.1.0";
    public string OrderType { get; set; } = "other";
    public int ExpireMinutes { get; set; } = 15;
}
