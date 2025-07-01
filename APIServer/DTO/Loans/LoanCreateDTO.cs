namespace APIServer.DTO.Loans
{
    public class LoanCreateDTO
    {
        public int UserId { get; set; }
        public int CopyId { get; set; }
        public DateTime DueDate { get; set; }
        public int? ReservationId { get; set; }
    }
}
