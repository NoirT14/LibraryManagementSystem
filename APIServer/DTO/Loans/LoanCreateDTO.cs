using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.Loans
{
    public class LoanCreateDTO
    {
        [Required(ErrorMessage = "UserId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "CopyId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "CopyId must be greater than 0")]
        public int CopyId { get; set; }

        [Required(ErrorMessage = "DueDate is required")]
        public DateTime DueDate { get; set; }

        public int? ReservationId { get; set; }
    }
}
