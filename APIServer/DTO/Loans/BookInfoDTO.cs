namespace APIServer.DTO.Loans
{
    public class BookInfoDTO
    {
        public int CopyId { get; set; }
        public string Barcode { get; set; }
        public string CopyStatus { get; set; }
        public string Location { get; set; }

        public int VariantId { get; set; }
        public int VolumeId { get; set; }//
        public int BookId { get; set; }

        public string Title { get; set; }
        public string VolumeTitle { get; set; }
        public List<string> Authors { get; set; }

        public string PublisherName { get; set; }
        public string EditionName { get; set; }
        public int? PublicationYear { get; set; }
        public string CoverTypeName { get; set; }
        public string PaperQualityName { get; set; }
        public decimal? Price { get; set; }
        public string ISBN { get; set; }
        public string Notes { get; set; }
    }
}
