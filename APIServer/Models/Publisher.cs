using System;
using System.Collections.Generic;

namespace APIServer.Models;

public partial class Publisher
{
    public int PublisherId { get; set; }

    public string PublisherName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Website { get; set; }

    public int? EstablishedYear { get; set; }

    public virtual ICollection<BookVariant> BookVariants { get; set; } = new List<BookVariant>();
}
