namespace MediConnect.Application.Configurations;

public class EmailSettings
{
    public bool IsEnabled { get; set; }
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "MediConnect";
    public int RetryCount { get; set; } = 2;
    public int RetryDelayMs { get; set; } = 400;
}