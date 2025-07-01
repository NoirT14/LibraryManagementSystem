using APIServer.DTO.Notification;

namespace APIServer.Service.Interfaces
{
    public interface INotificationService
    {
        Task<bool> TrackNotificationAsync(int notificationId);
        IQueryable<NotificationDTO> GetAllNotificationsQuery();
        Task<IEnumerable<NotificationDTO>> GetNotificationsByReceiverIdAsync(int receiverId);
        Task<bool> MarkAsReadAsync(int notificationId);

    }
}
