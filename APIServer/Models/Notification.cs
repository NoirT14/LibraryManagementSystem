using System;
using System.Collections.Generic;

namespace APIServer.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int? SenderId { get; set; }

    public string? SenderType { get; set; }

    public int? ReceiverId { get; set; }

    public bool? ForStaff { get; set; }

    public string Message { get; set; } = null!;

    public DateTime NotificationDate { get; set; }

    public string NotificationType { get; set; } = null!;

    public bool? ReadStatus { get; set; }

    public bool? HandledStatus { get; set; }

    public int? HandledBy { get; set; }

    public DateTime? HandledAt { get; set; }

    public string? RelatedTable { get; set; }

    public int? RelatedId { get; set; }

    public virtual User? HandledByNavigation { get; set; }

    public virtual User? Receiver { get; set; }
}
