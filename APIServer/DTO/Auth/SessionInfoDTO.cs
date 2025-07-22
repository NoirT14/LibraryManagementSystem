namespace APIServer.DTO.Auth
{
    public class SessionInfoDTO
    {
        public string SessionId { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public BrowserInfoDTO? BrowserInfo { get; set; }
    }
}
