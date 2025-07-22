using APIServer.DTO.Notification;

namespace APIServer.Service.Interfaces
{
    public interface INotificationService
    {
      
        IQueryable<NotificationDTO> GetAllNotificationsQuery();
        Task<IEnumerable<NotificationDTO>> GetNotificationsByReceiverIdAsync(int receiverId);
        Task<bool> MarkAsReadAsync(int notificationId);

        Task<int> CountUnreadNotificationsAsync(int notificationId);
    }
}
