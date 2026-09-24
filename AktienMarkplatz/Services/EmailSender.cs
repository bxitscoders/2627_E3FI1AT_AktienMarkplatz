using Microsoft.AspNetCore.Identity.UI.Services;

namespace AktienMarkplatz.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("E-Mail an {Email} ({Subject}):\n{Message}", email, subject, htmlMessage);
            return Task.CompletedTask;
        }
    }
}
