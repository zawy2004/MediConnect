using System.Net;
using System.Net.Mail;
using MediConnect.Application.Configurations;
using MediConnect.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediConnect.Infrastructure.Services;

public class EmailMessagingService : IEmailMessagingService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailMessagingService> _logger;

    public EmailMessagingService(
        IOptions<EmailSettings> options,
        ILogger<EmailMessagingService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<EmailSendResult> SendAsync(string toEmail, string subject, string body, bool isBodyHtml = false, CancellationToken cancellationToken = default)
    {
        if (!_settings.IsEnabled)
        {
            return new EmailSendResult
            {
                Success = false,
                ErrorMessage = "Email delivery is disabled in configuration."
            };
        }

        if (string.IsNullOrWhiteSpace(_settings.Host)
            || string.IsNullOrWhiteSpace(_settings.FromAddress)
            || string.IsNullOrWhiteSpace(_settings.Username)
            || string.IsNullOrWhiteSpace(_settings.Password))
        {
            return new EmailSendResult
            {
                Success = false,
                ErrorMessage = "Missing SMTP configuration values."
            };
        }

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            return new EmailSendResult
            {
                Success = false,
                ErrorMessage = "Recipient email is empty."
            };
        }

        var attempts = Math.Max(1, _settings.RetryCount + 1);
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.FromAddress, _settings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isBodyHtml
                };
                message.To.Add(toEmail);

                using var smtp = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 15000
                };

                await smtp.SendMailAsync(message, cancellationToken);
                return new EmailSendResult { Success = true };
            }
            catch (Exception ex)
            {
                if (attempt == attempts)
                {
                    _logger.LogError(ex, "Email send failed after {Attempts} attempts to {Recipient}.", attempt, toEmail);
                    return new EmailSendResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message
                    };
                }

                await Task.Delay(Math.Max(100, _settings.RetryDelayMs * attempt), cancellationToken);
            }
        }

        return new EmailSendResult
        {
            Success = false,
            ErrorMessage = "Failed to send email after retries."
        };
    }
}