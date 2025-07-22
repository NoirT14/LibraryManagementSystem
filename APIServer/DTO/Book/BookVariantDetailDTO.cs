namespace APIServer.DTO.Book
{
    public class BookVariantDetailDTO
    {
        public int VariantId { get; set; }
        public string Publisher { get; set; }
        public string ISBN { get; set; }
        public int? PublicationYear { get; set; }
        public string CoverType { get; set; }
        public string PaperQuality { get; set; }
        public decimal? Price { get; set; }
        public string Location { get; set; }
    }
}
