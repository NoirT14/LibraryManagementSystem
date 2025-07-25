﻿using System;
using System.Collections.Generic;

namespace APIServer.Models;

public partial class PaperQuality
{
    public int PaperQualityId { get; set; }

    public string PaperQualityName { get; set; } = null!;

    public virtual ICollection<BookVariant> BookVariants { get; set; } = new List<BookVariant>();
}
