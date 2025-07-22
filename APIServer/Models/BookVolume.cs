using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APIServer.Models;

public partial class BookVolume
{
    [Key]
    public int VolumeId { get; set; }

    public int BookId { get; set; }

    public int VolumeNumber { get; set; }

    public string? VolumeTitle { get; set; }

    public string? Description { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual ICollection<BookVariant> BookVariants { get; set; } = new List<BookVariant>();
}
