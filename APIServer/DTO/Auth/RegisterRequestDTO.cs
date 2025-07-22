namespace APIServer.DTO.Auth
{
    public class RegisterRequestDTO
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public BrowserInfoDTO? BrowserInfo { get; set; }
    }
}
