namespace APIServer.DTO.Auth
{
    public class LoginResponseDTO
    {
        public string Token { get; set; } = null!;
        public int Role { get; set; }
        public SessionInfoDTO? SessionInfo { get; set; }
    }
}
