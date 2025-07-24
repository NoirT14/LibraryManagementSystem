using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.Book
{
    public class BookHomepageDto
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<string> Authors { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool Available { get; set; }
    }
}

