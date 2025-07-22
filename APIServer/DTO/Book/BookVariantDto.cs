namespace APIServer.DTO.Book
{
    public class BookVariantDto
    {
        public int VariantId { get; set; }
        public string VolumeTitle { get; set; } = string.Empty;
        public int VolumeNumber { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public List<string> Authors { get; set; } = new();
    }
}
