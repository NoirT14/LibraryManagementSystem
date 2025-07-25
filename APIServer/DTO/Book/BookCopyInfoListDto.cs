namespace APIServer.DTO.Book
{
    public class BookCopyInfoListDto
    {
        public int CopyId { get; set; }
        public string? Barcode { get; set; }
        public string? CopyStatus { get; set; }
        public string? Location { get; set; }
        public int VariantId { get; set; }
        public int? PublicationYear { get; set; }
        public string? Isbn { get; set; }

        public int Volumn { get; set; }

        public string? CoverTypeName { get; set; }
        public string? PublisherName { get; set; }
        public string? EditionName { get; set; }
        public string? PaperQualityName { get; set; }

        public string BookTitle { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public List<string> AuthorNames { get; set; } = new();
    }
}
