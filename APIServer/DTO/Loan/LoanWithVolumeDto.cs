namespace APIServer.DTO.Loan
{
    public class LoanWithVolumeDto
    {
        public int LoanId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string LoanStatus { get; set; }
        public string? VolumeTitle { get; set; }
        public int? VolumeNumber { get; set; }
    }

}
