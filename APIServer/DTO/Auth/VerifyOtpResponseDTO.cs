namespace APIServer.DTO.Auth
{
    public class VerifyOtpResponseDTO
    {
        public string Email { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
