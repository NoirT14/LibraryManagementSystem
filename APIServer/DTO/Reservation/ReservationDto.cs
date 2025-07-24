namespace APIServer.DTO.Reservation
{
    public class ReservationDto
    {
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public int VariantId { get; set; }
        public string BookTitle { get; set; }
        public string Author { get; set; }
        public DateTime ReservationDate { get; set; }
        public string Status { get; set; }
    }
}
