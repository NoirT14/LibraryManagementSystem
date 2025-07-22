using APIServer.Data;
using APIServer.DTO.Loan;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Service
{
    public class LoanService : ILoanService
    {
        private readonly LibraryDatabaseContext _context;
        private readonly IEmailService _emailService;

        public LoanService(
            LibraryDatabaseContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task SendDueDateRemindersAsync()
        {
            var today = DateTime.Now.Date;
            var startOfDay = today;
            var endOfDay = today.AddDays(1);

            Console.WriteLine("📌 [Reminder] Bắt đầu gửi DueDateReminder...");

            var dueSoonLoans = await _context.Loans
                .Include(l => l.User)
                .Where(l => l.LoanStatus == "Borrowed"
                    && l.DueDate.Date >= today
                    && l.DueDate.Date <= today.AddDays(2))
                .ToListAsync();

            Console.WriteLine($"📊 [Reminder] Có {dueSoonLoans.Count} khoản mượn sắp đến hạn.");

            var userDueLoans = dueSoonLoans
                .GroupBy(l => l.UserId)
                .ToList();

            foreach (var group in userDueLoans)
            {
                var user = group.First().User;
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    Console.WriteLine($"⚠️ [Reminder] User {group.Key} thiếu thông tin hoặc email.");
                    continue;
                }

                Console.WriteLine($"🔔 [Reminder] Đang xử lý User {user.UserId} ({user.Email})...");

                var alreadySent = await _context.Notifications.AnyAsync(n =>
                    n.ReceiverId == user.UserId &&
                    n.NotificationType == "DueDateReminder" &&
                    n.NotificationDate >= startOfDay && n.NotificationDate < endOfDay
                );

                if (alreadySent)
                {
                    Console.WriteLine($"✅ [Reminder] User {user.UserId}: Đã gửi hôm nay, bỏ qua.");
                    continue;
                }

                var loanList = string.Join("", group.Select(l => $"- Loan ID: {l.LoanId}, Due on: {l.DueDate:yyyy-MM-dd}\n"));

                var plainBody = $@"Dear {user.FullName ?? "User"},

The following loans are due soon:
{loanList}
Please return the books on time to avoid fines.

Thank you.";

                var htmlLoanList = string.Join("", group.Select(l => $"<li>Loan ID: {l.LoanId}, Due on: {l.DueDate:yyyy-MM-dd}</li>"));

                var trackingUrl = "http://localhost:5027/api/notifications/track?notificationId=0";

                var htmlBodyTemplate = $@"<p>Dear {user.FullName ?? "User"},</p>
<p>The following loans are due soon:</p>
<ul>{htmlLoanList}</ul>
<p>Please return the books on time to avoid fines.</p>
<p>Please <a href='{trackingUrl}'>click here</a> to confirm reading this email.</p>
<p>Thank you.</p>";

                try
                {
                    Console.WriteLine($"📤 [Reminder] Gửi email đến {user.Email}...");
                    await _emailService.SendMailAsync(user.Email, "Library Due Date Reminder", htmlBodyTemplate);
                    Console.WriteLine("✅ [Reminder] Email đã gửi thành công.");

                    var notification = new Notification
                    {
                        SenderId = null,
                        SenderType = "System",
                        ReceiverId = user.UserId,
                        ForStaff = false,
                        Message = plainBody,
                        NotificationDate = DateTime.Now,
                        NotificationType = "DueDateReminder",
                        ReadStatus = false
                    };

                    Console.WriteLine("💾 [Reminder] Đang lưu notification vào database...");
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"✅ [Reminder] Đã lưu notification cho User {user.UserId}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ [Reminder] Lỗi khi gửi mail tới {user.Email}: {ex.Message}");
                }
            }
        }

        public async Task SendFineNotificationsAsync()
        {
            var today = DateTime.Now.Date;
            var startOfDay = today;
            var endOfDay = today.AddDays(1);

            var overdueLoansWithFines = await _context.Loans
                .Include(l => l.User)
                .Where(l => l.LoanStatus == "Overdue" && l.FineAmount > 0)
                .ToListAsync();

            foreach (var loan in overdueLoansWithFines)
            {
                var user = loan.User;
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    Console.WriteLine($"Loan {loan.LoanId}: Missing user or email.");
                    continue;
                }

                var alreadySent = await _context.Notifications.AnyAsync(n =>
                    n.ReceiverId == user.UserId &&
                    n.NotificationType == "FineNotice" &&
                    n.RelatedId == loan.LoanId &&
                    n.NotificationDate >= startOfDay && n.NotificationDate < endOfDay
                );

                if (alreadySent)
                {
                    Console.WriteLine($"User {user.UserId}, Loan {loan.LoanId}: Fine notice already sent today.");
                    continue;
                }

                var body = $@"Dear {user.FullName ?? "User"},

You have an outstanding fine of {loan.FineAmount:C} for an overdue loan (Loan ID: {loan.LoanId}).

Please pay the fine at your earliest convenience to avoid further action.

Thank you.";

                try
                {
                    await _emailService.SendMailAsync(user.Email, "Library Fine Notice", body);

                    _context.Notifications.Add(new Notification
                    {
                        SenderId = null,
                        SenderType = "System",
                        ReceiverId = user.UserId,
                        ForStaff = false,
                        Message = body,
                        NotificationDate = DateTime.Now,
                        NotificationType = "FineNotice",
                        RelatedTable = "loans",
                        RelatedId = loan.LoanId,
                        ReadStatus = false
                    });

                    await _context.SaveChangesAsync();

                    Console.WriteLine($"Fine notice sent and saved for User {user.UserId}, Loan {loan.LoanId}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending fine notice to {user.Email}: {ex.Message}");
                }
            }
        }

        public async Task UpdateOverdueLoansAndFinesAsync()
        {
            var now = DateTime.Now.Date;

            var overdueLoans = await _context.Loans
                .Where(l => l.LoanStatus == "Borrowed" && l.DueDate < now)
                .ToListAsync();

            foreach (var loan in overdueLoans)
            {
                var daysOverdue = (now - loan.DueDate.Date).Days;
                if (daysOverdue <= 0) continue;

                loan.LoanStatus = "Overdue";
                decimal fine = 5000 * daysOverdue;
                loan.FineAmount = fine;

                Console.WriteLine($"Loan {loan.LoanId}: Overdue {daysOverdue} day(s), fine = {fine}.");
            }

            await _context.SaveChangesAsync();
        }


        public async Task<List<LoanWithVolumeDto>> GetLoansWithVolumeByUserIdAsync(int userId)
        {
            return await _context.Loans
                .Where(l => l.UserId == userId)
                .Include(l => l.Copy)
                    .ThenInclude(c => c.Variant)
                        .ThenInclude(v => v.Volume)
                .Select(l => new LoanWithVolumeDto
                {
                    LoanId = l.LoanId,
                    BorrowDate = l.BorrowDate,
                    DueDate = l.DueDate,
                    ReturnDate = l.ReturnDate,
                    LoanStatus = l.LoanStatus,
                    VolumeTitle = l.Copy.Variant.Volume.VolumeTitle,
                    VolumeNumber = l.Copy.Variant.Volume.VolumeNumber
                })
                .ToListAsync();
        }


    }
}
