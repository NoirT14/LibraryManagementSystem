using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.Reservations
{
    public class ReservationUpdateDTO
    {
        [RegularExpression("^(Pending|Fulfilled|Cancelled|Expired)$", ErrorMessage = "Invalid reservation status")]
        public string? ReservationStatus { get; set; }

        public DateTime? ExpirationDate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ProcessedBy must be greater than 0")]
        public int? ProcessedBy { get; set; }
    }
}
