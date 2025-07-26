namespace APIServer.DTO.Book
{
    public class BookCopyRequest
    {
        public int VolumeId { get; set; }

        public int? PublisherId { get; set; }
        public int? EditionId { get; set; }
        public int? PublicationYear { get; set; }
        public int? CoverTypeId { get; set; }
        public int? PaperQualityId { get; set; }
        public string? Notes { get; set; }

        public string? CopyStatus { get; set; }
        public string? Location { get; set; }
    }
}
