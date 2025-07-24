namespace APIServer.DTO.Reservations
{
    public class ReservationListDTO
    {
        public int ReservationId { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string ReservationStatus { get; set; } = null!;
        public int QueuePosition { get; set; }

        // User info
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;

        // Book info
        public int VariantId { get; set; }
        public string Title { get; set; } = null!;
        public string? VolumeTitle { get; set; }
        public List<string> Authors { get; set; } = new List<string>();
        public string PublisherName { get; set; } = null!;
        public string ISBN { get; set; } = null!;

        // Availability info
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
    }
}
