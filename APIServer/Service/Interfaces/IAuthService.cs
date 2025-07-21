using APIServer.DTO.Auth;

namespace APIServer.Service.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> Authenticate(LoginRequestDTO loginRequest);
        Task<AuthResult> Register(RegisterRequestDTO registerRequest);
        Task<bool> SendResetPasswordTokenAsync(ForgotPasswordRequestDTO request);
        Task<AuthResult> ResetPassword(ResetPasswordRequestDTO resetRequest);
        Task<bool> IsEmailTaken(string email);
        Task<bool> IsUsernameTaken(string username);
    }
}
