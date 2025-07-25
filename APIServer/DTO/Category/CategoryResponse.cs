using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.Category
{
    public class CategoryResponse
    {
        [Key]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;

        public int BookCount { get; set; } = 0;
    }
}
