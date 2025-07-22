namespace APIServer.DTO.Author
{
    public class AuthorRespone
    {
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = null!;

        public string AuthorBio {  get; set; } = null!;
    }
}
