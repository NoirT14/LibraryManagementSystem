using System;
using System.Collections.Generic;

namespace APIServer.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateTime? CreateDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    public virtual ICollection<Notification> NotificationHandledByNavigations { get; set; } = new List<Notification>();

    public virtual ICollection<Notification> NotificationReceivers { get; set; } = new List<Notification>();

    public virtual ICollection<Reservation> ReservationProcessedByNavigations { get; set; } = new List<Reservation>();

    public virtual ICollection<Reservation> ReservationUsers { get; set; } = new List<Reservation>();

    public virtual Role Role { get; set; } = null!;
}
