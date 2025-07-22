using APIServer.DTO.Auth;

namespace APIServer.Service.Interfaces
{
    public interface IAuthService
    {
        // ✅ EXISTING: Authentication methods
        Task<AuthResult> Authenticate(LoginRequestDTO loginRequest, string ipAddress, string userAgent);
        Task<AuthResult> Register(RegisterRequestDTO registerRequest, string ipAddress, string userAgent);

        // ✅ EXISTING: Session management
        Task<bool> InvalidateSessionAsync(string sessionId);
        Task CleanupExpiredSessionsAsync();
        Task<Dictionary<string, object>> GetAnalyticsAsync();

        // ✅ NEW: OTP-based password reset methods
        Task<bool> SendResetPasswordOtpAsync(ForgotPasswordRequestDTO request);
        Task<AuthResult> VerifyOtpAsync(VerifyOtpRequestDTO request);
        Task<AuthResult> ResetPasswordWithOtpAsync(ResetPasswordWithOtpRequestDTO request);
        Task CleanupExpiredOtpsAsync();

        // ✅ LEGACY: Keep for backward compatibility (deprecated)
        Task<bool> SendResetPasswordTokenAsync(ForgotPasswordRequestDTO request);
        Task<AuthResult> ResetPassword(ResetPasswordRequestDTO resetRequest);

        // ✅ EXISTING: Validation methods
        Task<bool> IsEmailTaken(string email);
        Task<bool> IsUsernameTaken(string username);
    }
}