using System.ComponentModel.DataAnnotations;


namespace APIServer.DTO.Notification
{
    public class NotificationDTO
    {
        [Key]
        public int NotificationId { get; set; }

        public string Message { get; set; } = null!;

        public DateTime NotificationDate { get; set; }

        public bool? ReadStatus { get; set; }

        public string NotificationType { get; set; } = null!;

        public int? ReceiverId { get; set; }

        public string? RelatedTable { get; set; }

        public int? RelatedId { get; set; }

        // Optionals: nếu bạn cần thêm vào UI
        public bool? ForStaff { get; set; }

        public bool? HandledStatus { get; set; }

        public DateTime? HandledAt { get; set; }
    }
}
