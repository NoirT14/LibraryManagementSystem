using APIServer.Data;
using APIServer.Models;
using APIServer.Service.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace APIServer.Service
{
    public class LoanService : ILoanService
    {
        private readonly LibraryDatabaseContext _context;
        private readonly IConfiguration _config;

        public LoanService(LibraryDatabaseContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public Task BorrowBookAsync(int userId, int copyId)
        {
            throw new NotImplementedException();
        }

        public Task ReturnBookAsync(int loanId)
        {
            throw new NotImplementedException();
        }

        public async Task SendDueDateRemindersAsync()
        {
            var today = DateTime.Now.Date;
            var dueSoonLoans = await _context.Loans
                .Include(l => l.User)
                .Where(l => l.LoanStatus == "Borrowed"
                    && l.DueDate.Date >= today
                    && l.DueDate.Date <= today.AddDays(2))
                .ToListAsync();

            foreach (var loan in dueSoonLoans)
            {
                await SendEmailAsync(
                    loan.User.Email,
                    "Library Due Date Reminder",
                    $"Dear {loan.User.FullName},\n\nYour loan is due on {loan.DueDate:yyyy-MM-dd}. Please return the book on time to avoid fines.\n\nThank you."
                );
            }
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:FromEmail"]));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart("plain") { Text = body };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(
                _config["SmtpSettings:Server"],
                int.Parse(_config["SmtpSettings:Port"]),
                MailKit.Security.SecureSocketOptions.StartTls
            );
            await smtp.AuthenticateAsync(
                _config["SmtpSettings:User"],
                _config["SmtpSettings:Pass"]
            );
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
