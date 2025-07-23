namespace APIServer.DTO.User
{
    public class AdminResetPasswordRequestDTO
    {
        public int TargetUserId { get; set; }
        public string NewPassword { get; set; } = null!;
    }
}
