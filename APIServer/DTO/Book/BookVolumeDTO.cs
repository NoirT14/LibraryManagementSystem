using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.Book
{
    public class BookVolumeDTO
    {
        [Key]
        public int VolumeId { get; set; }
        public int BookId { get; set; }
        public string? VolumeTitle { get; set; }
        public int VolumeNumber { get; set; }
        public string? Description { get; set; }
        public string? BookTitle { get; set; }
    }
}
