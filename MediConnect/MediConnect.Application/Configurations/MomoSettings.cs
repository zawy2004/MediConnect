namespace MediConnect.Application.Configurations;

public class MomoSettings
{
    public string MomoApiUrl { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/create";
    public string SecretKey { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string DoctorReturnUrl { get; set; } = string.Empty;
    public string NotifyUrl { get; set; } = string.Empty;
    public string PartnerCode { get; set; } = "MOMO";
    public string RequestType { get; set; } = "payWithMethod";
    public int ExpireMinutes { get; set; } = 15;
}
