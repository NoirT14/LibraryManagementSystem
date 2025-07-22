namespace APIServer.DTO.User
{
    public class AdminUpdateUserRequestDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int RoleId { get; set; }
        public bool? IsActive { get; set; }
    }
}
