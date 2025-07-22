using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.Loans
{
    public class LoanEditDTO
    {
        [Required(ErrorMessage = "LoanStatus is required")]
        [RegularExpression("^(Borrowed|Returned|Overdue)$", ErrorMessage = "Invalid loan status")]
        public string LoanStatus { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "FineAmount cannot be negative")]
        public decimal FineAmount { get; set; }

        public bool Extended { get; set; }
    }

}
