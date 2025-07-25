using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.Book
{
    public class BookHomepageDto
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CoverImg { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Authors { get; set; } = new();
        public int VariantCount { get; set; }
    }
}

