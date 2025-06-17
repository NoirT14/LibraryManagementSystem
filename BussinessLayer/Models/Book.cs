using System;
using System.Collections.Generic;

namespace BussinessLayer.Models;

public partial class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public int CategoryId { get; set; }

    public string? Language { get; set; }

    public string? BookStatus { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<BookVolume> BookVolumes { get; set; } = new List<BookVolume>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();
}
