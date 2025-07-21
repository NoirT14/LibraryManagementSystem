namespace APIServer.DTO.Loans
{
    public class BookCopyDTO
    {
        public int CopyId { get; set; }
        public string Barcode { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string CopyStatus { get; set; } = null!;
        public int VariantId { get; set; }
    }
}
