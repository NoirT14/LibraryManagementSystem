namespace APIServer.DTO.Book
{
    public class BookDetailRespone
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public string? Language { get; set; }
        public string? BookStatus { get; set; }
        public string? Description { get; set; }
        public string? CoverImg { get; set; }

        public string CategoryName { get; set; } = null!;
        public List<string> AuthorNames { get; set; } = new();
        public List<BookVolumeDTO> Volumes { get; set; } = new();
        public List<BookVariantDetailDTO> Variants { get; set; }
    }
}
