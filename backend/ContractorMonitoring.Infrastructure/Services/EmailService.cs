using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ContractorMonitoring.Infrastructure.Services;

public interface IEmailService
{
    Task SendAsync(string subject, string body, string eventType, string? toAddress = null);
}

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string subject, string body, string eventType, string? toAddress = null)
    {
        var section = _config.GetSection("Email");
        var host     = section["SmtpHost"];
        var port     = int.Parse(section["SmtpPort"] ?? "587");
        var user     = section["SmtpUser"] ?? string.Empty;
        var password = section["SmtpPassword"] ?? string.Empty;
        var from     = section["FromAddress"] ?? "noreply@contractor.monitoring";
        var fromName = section["FromName"] ?? "Contractor Monitoring System";
        var enableSsl = bool.Parse(section["EnableSsl"] ?? "true");

        // Default recipient: system admin address from config, or override
        var to = toAddress ?? section["AdminEmail"] ?? from;

        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogDebug("Email skipped for event {EventType}: SmtpHost not configured", eventType);
            return;
        }

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = string.IsNullOrEmpty(user)
                    ? null
                    : new NetworkCredential(user, password)
            };

            var message = new MailMessage
            {
                From       = new MailAddress(from, fromName),
                Subject    = subject,
                Body       = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent for event {EventType} to {To}", eventType, to);
        }
        catch (SmtpException ex) when (ex.InnerException is System.Net.Sockets.SocketException)
        {
            // SMTP server unreachable — expected in dev when no mail server is running
            _logger.LogWarning("Email skipped for event {EventType}: SMTP server {Host}:{Port} unreachable", eventType, host, port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email for event {EventType}", eventType);
        }
    }
}
