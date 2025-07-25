namespace APIServer.DTO.Author
{
    public class AuthorRequest
    {
        public string AuthorName { get; set; } = null!;

        public string AuthorBio { get; set; } = null!;

        public string? Nationality { get; set; }

        public string? Genre { get; set; }

        public IFormFile? Photo { get; set; }
    }
}
