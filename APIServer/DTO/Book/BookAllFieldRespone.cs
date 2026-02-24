namespace APIServer.DTO.Book
{
    public class BookAllFieldRespone
    {
        public int BookId { get; set; }
        public string Title { get; set; } = null!;
        public string? Language { get; set; }
        public string? BookStatus { get; set; }
        public string? Description { get; set; }
        public string? CoverImg { get; set; }
        public string CategoryName { get; set; } = null!;
        public List<string> Authors { get; set; } = new();

        public List<VolumeDto> Volumes { get; set; } = new();

        public class VolumeDto
        {
            public int VolumeId { get; set; }
            public int VolumeNumber { get; set; }
            public string? VolumeTitle { get; set; }
            public string? Description { get; set; }

            public List<VariantDto> Variants { get; set; } = new();
        }

        public class VariantDto
        {
            public int VariantId { get; set; }
            public int? PublicationYear { get; set; }
            public string? Isbn { get; set; }
            public string? EditionName { get; set; }
            public string? PublisherName { get; set; }
            public string? CoverTypeName { get; set; }
            public string? PaperQualityName { get; set; }

            public List<CopyDto> Copies { get; set; } = new();
        }

        public class CopyDto
        {
            public int CopyId { get; set; }
            public string? Barcode { get; set; }
            public string? CopyStatus { get; set; }
            public string? Location { get; set; }
        }
    }
}
