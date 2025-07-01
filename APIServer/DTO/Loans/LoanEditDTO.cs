namespace APIServer.DTO.Loans
{
    public class LoanEditDTO
    {
        public string LoanStatus { get; set; }
        public decimal FineAmount { get; set; }
        public bool Extended { get; set; }
    }

}
