namespace APIServer.DTO.Reservations
{
    public class BookAvailabilityDTO
    {
        public int VariantId { get; set; }
        public string Title { get; set; } = null!;
        public string? VolumeTitle { get; set; }
        public List<string> Authors { get; set; } = new List<string>();
        public string PublisherName { get; set; } = null!;
        public string ISBN { get; set; } = null!;
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int PendingReservations { get; set; }
        public bool CanReserve { get; set; }
    }
}
