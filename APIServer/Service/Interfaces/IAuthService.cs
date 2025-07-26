using APIServer.DTO.Auth;

namespace APIServer.Service.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> Authenticate(LoginRequestDTO loginRequest, string ipAddress, string userAgent);
        Task<AuthResult> Register(RegisterRequestDTO registerRequest, string ipAddress, string userAgent);

        Task<bool> SendResetPasswordOtpAsync(ForgotPasswordRequestDTO request);
        Task<AuthResult> VerifyOtpAsync(VerifyOtpRequestDTO request);
        Task<AuthResult> ResetPasswordWithOtpAsync(ResetPasswordWithOtpRequestDTO request);
        Task CleanupExpiredOtpsAsync();
        Task<bool> SendResetPasswordTokenAsync(ForgotPasswordRequestDTO request);
        Task<AuthResult> ResetPassword(ResetPasswordRequestDTO resetRequest);
        Task<bool> IsEmailTaken(string email);
        Task<bool> IsUsernameTaken(string username);
        Task<Dictionary<string, object>> GetAnalyticsAsync();
        Task<bool> InvalidateSessionAsync(string sessionId);
        Task CleanupExpiredSessionsAsync();
        Task<bool> ValidateSessionFingerprintAsync(string sessionId, BrowserInfoDTO currentBrowserInfo);
    }
}