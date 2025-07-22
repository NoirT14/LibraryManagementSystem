namespace APIServer.DTO.Book
{
    public class BookInfoRequest
    {
        public string Title { get; set; } = null!;

        public int CategoryId { get; set; }

        public string? Language { get; set; }

        public string? BookStatus { get; set; }

        public string? Description { get; set; }
        public List<int> AuthorIds { get; set; } = new();
    }
}
