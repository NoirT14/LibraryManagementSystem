using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.Book
{
    public class BookVolumeDTO
    {
        [Key]
        public int VolumeId { get; set; }
        public int VolumeNumber { get; set; }
        public string? VolumeTitle { get; set; }
        public string? Description { get; set; }
    }
}
