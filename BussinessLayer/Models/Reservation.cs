using System;
using System.Collections.Generic;

namespace BussinessLayer.Models;

public partial class Reservation
{
    public int ReservationId { get; set; }

    public int UserId { get; set; }

    public int VariantId { get; set; }

    public DateTime ReservationDate { get; set; }

    public DateTime? ExpirationDate { get; set; }

    public string ReservationStatus { get; set; } = null!;

    public int? FulfilledCopyId { get; set; }

    public int? ProcessedBy { get; set; }

    public virtual BookCopy? FulfilledCopy { get; set; }

    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    public virtual User? ProcessedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual BookVariant Variant { get; set; } = null!;
}
