namespace APIServer.DTO.Book
{
    public class UpdateCopyAndVariantRequest
    {
        public int? CoverTypeId { get; set; }
        public int? PaperQualityId { get; set; }
        public int? EditionId { get; set; }
        public string? Location { get; set; }
        public string? CopyStatus { get; set; }
    }
}
