using System.Threading.Tasks;

namespace MiniMarketApp.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
