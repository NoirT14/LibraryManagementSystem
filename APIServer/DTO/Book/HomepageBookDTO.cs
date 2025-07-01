using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.Book
{
    public class HomepageBookDTO
    {
        [Key]
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Language { get; set; }
        public string? Status { get; set; }
        public string? CategoryName { get; set; }
        public List<string> Authors { get; set; } = new();
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
    }
}
