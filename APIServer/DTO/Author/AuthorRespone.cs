using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.Author
{
    public class AuthorRespone
    {
        [Key]
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = null!;

        public string AuthorBio {  get; set; } = null!;

        public string? Nationality { get; set; }

        public string? Genre { get; set; }

        public string? PhotoUrl { get; set; }

        public int BookCount { get; set; }
    }
}
