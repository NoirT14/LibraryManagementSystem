namespace APIServer.DTO.Reservation
{
    public class ReservationInfoListRespone
    {
        public int ReservationId { get; set; }
        public string BookTitle { get; set; } = null!;
        public string VolumeTitle { get; set; } = null!;
        public string ReservedDate { get; set; } = null!; 
        public string Status { get; set; } = null!;
    }
}
