using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.Book
{
    public class BookDetailDTO
    {
        [Key]
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Language { get; set; }
        public string? Status { get; set; }
        public string? CategoryName { get; set; }
        public List<string> Authors { get; set; } = new();

        public List<VariantInfo> Variants { get; set; } = new();

        public class VariantInfo
        {
            public int VariantId { get; set; }
            public int PublicationYear { get; set; }
            public string? ISBN { get; set; }
            public string? PublisherName { get; set; }
            public string? CoverTypeName { get; set; }
            public string? PaperQualityName { get; set; }
            public int TotalCopies { get; set; }
            public int AvailableCopies { get; set; }
        }
    }
}
