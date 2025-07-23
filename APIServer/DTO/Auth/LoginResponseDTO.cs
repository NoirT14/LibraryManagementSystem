namespace APIServer.DTO.Auth
{
    public class LoginResponseDTO
    {
        public string Token { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public int Role { get; set; }
    }
}
