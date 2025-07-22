namespace APIServer.DTO.Book
{
    public class BookInfoRespone
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public string? Language { get; set; }
        public string? BookStatus { get; set; }
        public string? Description { get; set; }

        public string? Author { get; set; }

        public string? Volumn { get; set; }

        public string? Availability {  get; set; }

        public string? CoverImg { get; set; }
    }
}
