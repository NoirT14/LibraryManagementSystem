using System;
using System.Collections.Generic;

namespace APIServer.Models;

public partial class Loan
{
    public int LoanId { get; set; }

    public int UserId { get; set; }

    public int CopyId { get; set; }

    public DateTime BorrowDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public string LoanStatus { get; set; } = null!;

    public decimal? FineAmount { get; set; }

    public bool? Extended { get; set; }

    public int? ReservationId { get; set; }

    public virtual BookCopy Copy { get; set; } = null!;

    public virtual Reservation? Reservation { get; set; }

    public virtual User User { get; set; } = null!;
}
