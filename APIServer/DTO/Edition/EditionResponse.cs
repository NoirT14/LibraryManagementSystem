using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.Edition
{
    public class EditionResponse
    {
        [Key]
        public int EditionId { get; set; }
        public string EditionName { get; set; } = null!;
        public int BookCount { get; set; } = 0;
    }
}
