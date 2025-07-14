using APIServer.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace APIServer.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendMailAsync(string toEmail, string subject, string body)
        {
            var gmailConfig = _configuration.GetSection("Gmail");

            string? host = gmailConfig.GetValue<string>("Host");
            int? port = gmailConfig.GetValue<int?>("Port");
            bool? enableSsl = gmailConfig.GetValue<bool?>("EnableSsl");
            string? username = gmailConfig.GetValue<string>("Username");

            if (string.IsNullOrEmpty(host) || port == null || enableSsl == null || string.IsNullOrEmpty(username))
            {
                throw new InvalidOperationException("Invalid Gmail configuration settings.");
            }

            string? password = gmailConfig.GetValue<string>("Password");

            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Gmail password is missing in config.");
            }

            using var client = new SmtpClient(host, port.Value)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl.Value
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username, "DevTeam"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}