using System;
using System.Collections.Generic;

namespace APIServer.Models;

public partial class BookVariant
{
    public int VariantId { get; set; }

    public int VolumeId { get; set; }

    public int? PublisherId { get; set; }

    public int? EditionId { get; set; }

    public int? PublicationYear { get; set; }

    public int? CoverTypeId { get; set; }

    public int? PaperQualityId { get; set; }

    public decimal? Price { get; set; }

    public string? Isbn { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();

    public virtual CoverType? CoverType { get; set; }

    public virtual Edition? Edition { get; set; }

    public virtual PaperQuality? PaperQuality { get; set; }

    public virtual Publisher? Publisher { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual BookVolume Volume { get; set; } = null!;
}
