using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.Reservations
{
    public class ReservationCreateDTO
    {
        [Required(ErrorMessage = "UserId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "VariantId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "VariantId must be greater than 0")]
        public int VariantId { get; set; }

        public DateTime? ExpirationDate { get; set; }
    }
}
