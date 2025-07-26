using System;
using System.Collections.Generic;

namespace APIServer.Models;

public partial class Author
{
    public int AuthorId { get; set; }

    public string AuthorName { get; set; } = null!;

    public string? Bio { get; set; } = null!;

    public string? Nationality { get; set; }

    public string? Genre { get; set; }

    public string? PhotoUrl { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
