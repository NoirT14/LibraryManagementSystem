using APIServer.Service.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace APIServer.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            var fromEmail = _config["Gmail:Username"];
            var host = _config["Gmail:Host"];
            var port = int.Parse(_config["Gmail:Port"]);
            var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("SMTP credentials missing.");
            }

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Library System", fromEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(isHtml ? "html" : "plain") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(fromEmail, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            Console.WriteLine($"📧 Email sent to {to}");
        }
    }
}
