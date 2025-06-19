using System;
using System.Collections.Generic;

namespace APIServer.Models;

public partial class BookCopy
{
    public int CopyId { get; set; }

    public int VariantId { get; set; }

    public string? Barcode { get; set; }

    public string? CopyStatus { get; set; }

    public string? Location { get; set; }

    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual BookVariant Variant { get; set; } = null!;
}
