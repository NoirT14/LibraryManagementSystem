namespace APIServer.DTO.Loans
{
    public class LoanListDTO
    {
        public int LoanId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string LoanStatus { get; set; }
        public decimal FineAmount { get; set; }
        public bool Extended { get; set; }

        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public string Barcode { get; set; }
        public string Title { get; set; }
        public string VolumeTitle { get; set; }
        public List<string> Authors { get; set; }
    }

}
