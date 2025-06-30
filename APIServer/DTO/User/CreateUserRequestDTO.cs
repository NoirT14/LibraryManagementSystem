namespace APIServer.DTO.User
{
    public class CreateUserRequestDTO
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
