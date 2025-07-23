using APIServer.Data;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Service
{
    public class ReservationService : IReservationService
    {
        private readonly LibraryDatabaseContext _context;
        private readonly IEmailService _emailService;

        public ReservationService(
            LibraryDatabaseContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // 1. Đặt sách
        public async Task CreateReservationAsync(int userId, int variantId)
        {
            var reservation = new Reservation
            {
                UserId = userId,
                VariantId = variantId,
                ReservationDate = DateTime.Now,
                ReservationStatus = "Pending"
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var message = "Bạn đã đặt sách thành công. Vui lòng chờ khi sách có sẵn.";

            _context.Notifications.Add(new Notification
            {
                ReceiverId = userId,
                Message = message,
                NotificationDate = DateTime.Now,
                NotificationType = "BorrowConfirmed",
                RelatedTable = "reservations",
                RelatedId = reservation.ReservationId,
                ReadStatus = false,
                SenderType = "System"
            });

            await _context.SaveChangesAsync();

            Console.WriteLine($"Reservation created and notification sent for User {userId}");
        }

        // 2. Khi có bản sao trả về: Gửi noti + email
        public async Task CheckAvailableReservationsAsync()
        {
            var availableCopies = await _context.BookCopies
                .Include(c => c.Variant) // có thể cần nếu hiển thị thêm thông tin
                .Where(c => c.CopyStatus == "Available")
                .ToListAsync();

            foreach (var copy in availableCopies)
            {
                // Tìm đơn đặt giữ đầu tiên của variant tương ứng, trạng thái "Pending"
                var reservation = await _context.Reservations
                    .Include(r => r.User)
                    .Where(r => r.VariantId == copy.VariantId && r.ReservationStatus == "Pending")
                    .OrderBy(r => r.ReservationDate)
                    .FirstOrDefaultAsync();

                // Nếu không có ai đặt, thì giữ bản sao đó ở trạng thái Available
                if (reservation == null) continue;

                // Gán bản sao cho người đặt đầu tiên
                reservation.ReservationStatus = "Available";
                reservation.ExpirationDate = DateTime.Now.AddDays(2);
                reservation.FulfilledCopyId = copy.CopyId;

                // Cập nhật trạng thái bản sao thành Reserved
                copy.CopyStatus = "Reserved";

                // Thêm thông báo vào bảng Notification
                var message = $"Sách bạn đặt (Mã: {reservation.VariantId}) đã có sẵn. Vui lòng đến nhận trong 2 ngày.";

                _context.Notifications.Add(new Notification
                {
                    ReceiverId = reservation.UserId,
                    Message = message,
                    NotificationDate = DateTime.Now,
                    NotificationType = "ReservationAvailable",
                    RelatedTable = "reservations",
                    RelatedId = reservation.ReservationId,
                    ReadStatus = false,
                    SenderType = "System"
                });

                // Lưu tất cả thay đổi: copy + reservation + notification
                await _context.SaveChangesAsync();

                Console.WriteLine($"Reservation {reservation.ReservationId} is now Available and notified.");

                // Gửi email nếu có
                if (!string.IsNullOrEmpty(reservation.User?.Email))
                {
                    try
                    {
                        var emailBody = $@"
                    <p>Chào {reservation.User.FullName ?? "bạn"},</p>
                    <p>Sách bạn đặt (Mã sách: <strong>{reservation.VariantId}</strong>) đã có sẵn.</p>
                    <p>Vui lòng đến thư viện nhận trong vòng <strong>2 ngày</strong>.</p>
                    <p>Xin cảm ơn.</p>";

                        await _emailService.SendMailAsync(
                            reservation.User.Email,
                            "Thông báo đặt sách đã sẵn sàng",
                            emailBody
                        );

                        Console.WriteLine($"Email sent to {reservation.User.Email}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send email to {reservation.User.Email}: {ex.Message}");
                    }
                }

                // ⚠️ Sau khi 1 bản copy đã được gán, ta không dùng lại nó nữa, tiếp tục sang bản copy khác
            }
        }


        // 3. Hết hạn giữ – Gửi noti + email
        public async Task ExpireOldReservationsAsync()
        {
            var now = DateTime.Now;
            var expired = await _context.Reservations
                .Include(r => r.User)
                .Where(r => r.ReservationStatus == "Available" &&
                            r.ExpirationDate < now)
                .ToListAsync();

            foreach (var reservation in expired)
            {
                reservation.ReservationStatus = "Expired";

                var message = $"Đơn đặt sách (Mã: {reservation.ReservationId}) của bạn đã hết hạn giữ.";

                _context.Notifications.Add(new Notification
                {
                    ReceiverId = reservation.UserId,
                    Message = message,
                    NotificationDate = now,
                    NotificationType = "ReservationCanceled",
                    RelatedTable = "reservations",
                    RelatedId = reservation.ReservationId,
                    ReadStatus = false,
                    SenderType = "System"
                });

                // Gửi email nếu có
                if (!string.IsNullOrEmpty(reservation.User?.Email))
                {
                    try
                    {
                        var emailBody = $@"
                            <p>Chào {reservation.User.FullName ?? "bạn"},</p>
                            <p>Đơn đặt sách (Mã đơn: <strong>{reservation.ReservationId}</strong>) đã hết hiệu lực do bạn không đến nhận đúng thời gian.</p>
                            <p>Nếu bạn vẫn muốn mượn sách, vui lòng đặt lại.</p>
                            <p>Xin cảm ơn.</p>";

                        await _emailService.SendMailAsync(
                            reservation.User.Email,
                            "Thông báo đặt sách hết hạn",
                            emailBody
                        );

                        Console.WriteLine($"Email sent to {reservation.User.Email}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send email to {reservation.User.Email}: {ex.Message}");
                    }
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"Expired {expired.Count} reservations and sent notifications.");
        }

        public async Task<int> CountReservationsAsync()
        {
            return await _context.Reservations.CountAsync();
        }

        public async Task<int> CountByStatusAsync(string status)
        {
            return await _context.Reservations.CountAsync(r => r.ReservationStatus == status);
        }
    }
}
