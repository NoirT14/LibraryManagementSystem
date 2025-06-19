using System;
using System.Collections.Generic;

namespace APIServer.Models;

public partial class CoverType
{
    public int CoverTypeId { get; set; }

    public string CoverTypeName { get; set; } = null!;

    public virtual ICollection<BookVariant> BookVariants { get; set; } = new List<BookVariant>();
}
