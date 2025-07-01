namespace APIServer.DTO.Book
{
    public class BookVariantReservationDTO
    {
        public int VariantId { get; set; }
        public string Title { get; set; } = default!;
        public string? ISBN { get; set; }
        public int? PublicationYear { get; set; }
    }
}
