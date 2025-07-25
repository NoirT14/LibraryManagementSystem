namespace APIServer.DTO.Publisher
{
    public class PublisherRequest
    {
        public string PublisherName { get; set; } = null!;

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public string? Website { get; set; }

        public int? EstablishedYear { get; set; }
    }

}
