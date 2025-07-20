namespace APIServer.DTO.User
{
    public class UserProfileUpdateRequestDTO
    {
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
