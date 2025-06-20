using System;
using System.Collections.Generic;

namespace APIServer.Models;

public partial class Edition
{
    public int EditionId { get; set; }

    public string EditionName { get; set; } = null!;

    public virtual ICollection<BookVariant> BookVariants { get; set; } = new List<BookVariant>();
}
