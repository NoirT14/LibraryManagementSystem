using System.ComponentModel.DataAnnotations;

namespace APIServer.DTO.User
{
    public class AdminUserResponseDTO
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string RoleName { get; set; } = null!;
        public bool? IsActive { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
