using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.CoverType
{
    public class CoverTypeResponse
    {
        [Key]
        public int CoverTypeId { get; set; }
        public string CoverTypeName { get; set; } = null!;

        public int BookCount { get; set; } = 0;
    }
}
