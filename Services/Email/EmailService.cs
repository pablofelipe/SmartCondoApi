using System.Net.Mail;
using System.Net;

namespace SmartCondoApi.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfigurationSection emailSettings = null;

        public EmailService(IConfiguration _configuration)
        {
            emailSettings = _configuration.GetSection("EmailSettings");
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient(emailSettings["SmtpServer"], Convert.ToInt32(emailSettings["SmtpPort"]))
            {
                Credentials = new NetworkCredential(emailSettings["FromEmail"], emailSettings["FromPassword"]),
                EnableSsl = Convert.ToBoolean(emailSettings["EnableSsl"])
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(emailSettings["FromEmail"]),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }
}
