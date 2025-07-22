namespace APIServer.DTO.Auth
{
    public class BrowserInfoDTO
    {
        public string? BrowserName { get; set; }
        public string? BrowserVersion { get; set; }
        public string? OperatingSystem { get; set; }
        public string? Language { get; set; }
        public string? Timezone { get; set; }
        public string? ScreenResolution { get; set; }
        public string? UserAgent { get; set; }
    }
}
