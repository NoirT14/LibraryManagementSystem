namespace APIServer.DTO.Book
{
    public class BookInfoListSearchCopyRespone
    {
        public int VolumeId { get; set; }            
        public int VolumeNumber { get; set; }        
        public string Title { get; set; } = null!;    
        public string? Author { get; set; }           
        public string? CoverImg {get; set;}
    }
}
