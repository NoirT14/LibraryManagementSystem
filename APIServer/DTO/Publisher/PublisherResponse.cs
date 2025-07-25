using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.Publisher
{
    public class PublisherResponse
    {
        [Key]
        public int PublisherId { get; set; }
        public string PublisherName { get; set; } = null!;
        public int BookCount { get; set; } = 0;

        public string? Address { get; set; }

        public string? Phone { get; set; }

        public string? Website { get; set; }

        public int? EstablishedYear { get; set; }
    }
}
