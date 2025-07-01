using APIServer.Data;
using APIServer.DTO.Notification;
using APIServer.Models;
using APIServer.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace APIServer.Service
{
    public class NotificationService : INotificationService
    {
        private readonly LibraryDatabaseContext _context;

        public NotificationService(LibraryDatabaseContext context)
        {
            _context = context;
        }
        public IQueryable<NotificationDTO> GetAllNotificationsQuery()
        {
            return _context.Notifications
                .Select(n => new NotificationDTO
                {
                    NotificationId = n.NotificationId,
                    Message = n.Message,
                    NotificationDate = n.NotificationDate,
                    NotificationType = n.NotificationType,
                    ReadStatus = n.ReadStatus ?? false,
                    ReceiverId = n.ReceiverId,
                    RelatedTable = n.RelatedTable,
                    RelatedId = n.RelatedId,
                    ForStaff = n.ForStaff,
                    HandledStatus = n.HandledStatus,
                    HandledAt = n.HandledAt
                });
        }
        public async Task<IEnumerable<NotificationDTO>> GetNotificationsByReceiverIdAsync(int receiverId)
        {
            var query = await _context.Notifications
                .Where(n => n.ReceiverId == receiverId)
                .OrderByDescending(n => n.NotificationDate)
                .Select(n => new NotificationDTO
                {
                    NotificationId = n.NotificationId,
                    Message = n.Message,
                    NotificationDate = n.NotificationDate,
                    NotificationType = n.NotificationType,
                    ReadStatus = n.ReadStatus,
                    ReceiverId = n.ReceiverId,
                    RelatedTable = n.RelatedTable,
                    RelatedId = n.RelatedId,
                    ForStaff = n.ForStaff,
                    HandledStatus = n.HandledStatus,
                    HandledAt = n.HandledAt
                })
                .ToListAsync();

            return query;
        }


        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
                return false;

            if (notification.ReadStatus == true)
                return true; // Đã đọc rồi, không cần làm gì

            notification.ReadStatus = true;
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> TrackNotificationAsync(int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            if (notification == null)
                return false;

            if (notification.ReadStatus == true)
                return true; // Đã đọc rồi

            notification.ReadStatus = true;
            await _context.SaveChangesAsync();
            Console.WriteLine($"Tracked read for notification {notificationId}.");
            return true;
        }
    }
}
