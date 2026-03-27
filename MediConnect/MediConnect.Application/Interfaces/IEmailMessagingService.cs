namespace MediConnect.Application.Interfaces;

public interface IEmailMessagingService
{
    Task<EmailSendResult> SendAsync(string toEmail, string subject, string body, bool isBodyHtml = false, CancellationToken cancellationToken = default);
}

public class EmailSendResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}