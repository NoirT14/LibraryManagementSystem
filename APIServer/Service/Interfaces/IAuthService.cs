using APIServer.DTO.Auth;

namespace APIServer.Service.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> Authenticate(LoginRequestDTO loginRequest);
        Task<bool> SendResetPasswordTokenAsync(ForgotPasswordRequestDTO request);
        Task<AuthResult> ResetPassword(ResetPasswordRequestDTO resetRequest);
    }
}
