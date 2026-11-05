using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MiniMarketApp.Settings;

namespace MiniMarketApp.Services;

public class GmailEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<GmailEmailSender> _logger;

    public GmailEmailSender(IOptions<EmailSettings> settings, ILogger<GmailEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (string.IsNullOrWhiteSpace(_settings.SenderEmail) || 
            _settings.SenderEmail.Contains("tu-correo") || 
            _settings.SenderEmail.Contains("su_correo"))
        {
            _logger.LogWarning("EmailSender: Configuración SMTP no iniciada o con valores temporales. Omitiendo envío de correo.");
            return;
        }

        try
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                _settings.SenderName,
                _settings.SenderEmail));

            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("EmailSender: Correo enviado exitosamente a {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EmailSender: Error al enviar correo SMTP a {Email}. Detalle: {Message}", email, ex.Message);
            // Capturar la excepción de forma segura para evitar caída de 500 en el registro de usuarios
        }
    }
}
