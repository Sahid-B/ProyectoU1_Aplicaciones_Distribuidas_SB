using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace MiniMarketApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IEmailSender _emailSender;

        public EmailService(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await _emailSender.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
