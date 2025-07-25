using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.PaperQuality
{
    public class PaperQualityResponse
    {
        [Key]
        public int PaperQualityId { get; set; }
        public string PaperQualityName { get; set; } = null!;
        public int BookCount { get; set; } = 0;
    }
}
