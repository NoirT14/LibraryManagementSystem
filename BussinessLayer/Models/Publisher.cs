using System;
using System.Collections.Generic;

namespace BussinessLayer.Models;

public partial class Publisher
{
    public int PublisherId { get; set; }

    public string PublisherName { get; set; } = null!;

    public virtual ICollection<BookVariant> BookVariants { get; set; } = new List<BookVariant>();
}
