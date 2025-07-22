using APIServer.DTO.Auth;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            try
            {
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();

                // ✅ Enhanced logging
                _logger.LogInformation("Login attempt for {UsernameOrEmail} from {IpAddress} using {UserAgent}",
                    request.UsernameorEmail, ipAddress, userAgent);

                var result = await _authService.Authenticate(request, ipAddress, userAgent);

                if (!result.IsSuccess)
                {
                    return Unauthorized(new { message = result.ErrorMessage });
                }

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {UsernameOrEmail}", request.UsernameorEmail);
                return StatusCode(500, new { message = "An error occurred during login." });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO request)
        {
            try
            {
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();

                _logger.LogInformation("Registration attempt for {Username}/{Email} from {IpAddress}",
                    request.Username, request.Email, ipAddress);

                if (await _authService.IsUsernameTaken(request.Username))
                {
                    return BadRequest(new
                    {
                        isSuccess = false,
                        errorMessage = "Tên đăng nhập đã tồn tại"
                    });
                }

                var emailExists = await _authService.IsEmailTaken(request.Email);
                if (emailExists)
                {
                    return BadRequest(new
                    {
                        isSuccess = false,
                        errorMessage = "Email đã được sử dụng"
                    });
                }

                var result = await _authService.Register(request, ipAddress, userAgent);
                if (!result.IsSuccess) return BadRequest(result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Username}", request.Username);
                return StatusCode(500, new { message = "An error occurred during registration." });
            }
        }

        // ✅ UPDATED: Now sends OTP instead of reset link
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ipAddress = GetClientIpAddress();
                _logger.LogInformation("Forgot password request for {UsernameOrEmail} from {IpAddress}",
                    request.UsernameorEmail, ipAddress);

                var result = await _authService.SendResetPasswordOtpAsync(request);

                // Always return success to prevent email enumeration attacks
                return Ok(new
                {
                    message = "Nếu email tồn tại trong hệ thống, mã OTP đã được gửi để đặt lại mật khẩu.",
                    isSuccess = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for {UsernameOrEmail}", request.UsernameorEmail);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu." });
            }
        }

        // ✅ NEW: Verify OTP endpoint
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ipAddress = GetClientIpAddress();
                _logger.LogInformation("OTP verification attempt for {Email} from {IpAddress}",
                    request.Email, ipAddress);

                var result = await _authService.VerifyOtpAsync(request);

                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        message = "Mã OTP hợp lệ. Bạn có thể đặt lại mật khẩu.",
                        isSuccess = true,
                        data = result.Data
                    });
                }

                return BadRequest(new
                {
                    message = result.ErrorMessage,
                    isSuccess = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OTP verification for {Email}", request.Email);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xác minh OTP." });
            }
        }

        // ✅ NEW: Reset password with OTP
        [HttpPost("reset-password-with-otp")]
        public async Task<IActionResult> ResetPasswordWithOtp([FromBody] ResetPasswordWithOtpRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var ipAddress = GetClientIpAddress();
                _logger.LogInformation("Password reset with OTP attempt for {Email} from {IpAddress}",
                    request.Email, ipAddress);

                var result = await _authService.ResetPasswordWithOtpAsync(request);

                if (result.IsSuccess)
                {
                    return Ok(new
                    {
                        message = "Mật khẩu đã được đặt lại thành công!",
                        isSuccess = true
                    });
                }

                return BadRequest(new
                {
                    message = result.ErrorMessage,
                    isSuccess = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset with OTP for {Email}", request.Email);
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi đặt lại mật khẩu." });
            }
        }

        // ✅ LEGACY: Keep old reset password for backward compatibility (deprecated)
        [HttpPost("reset-password")]
        [Obsolete("This endpoint is deprecated. Use reset-password-with-otp instead.")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            return BadRequest(new
            {
                message = "Phương thức này không còn được hỗ trợ. Vui lòng sử dụng đặt lại mật khẩu bằng OTP.",
                isSuccess = false,
                deprecated = true
            });
        }

        [HttpGet("analytics")]
        [Authorize]
        public async Task<IActionResult> GetAnalytics()
        {
            try
            {
                var analytics = await _authService.GetAnalyticsAsync();
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analytics data");
                return StatusCode(500, new { message = "An error occurred while retrieving analytics." });
            }
        }

        [HttpPost("logout")]
        [Authorize] // ✅ Require valid JWT to logout
        public async Task<IActionResult> Logout()
        {
            try
            {
                // ✅ Extract session info from JWT claims
                var sessionId = User.FindFirst("sessionId")?.Value;
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                var ipAddress = GetClientIpAddress();

                if (string.IsNullOrEmpty(sessionId))
                {
                    return BadRequest(new
                    {
                        message = "Invalid session information",
                        isSuccess = false
                    });
                }

                // ✅ Invalidate session on server
                var result = await _authService.InvalidateSessionAsync(sessionId);

                _logger.LogInformation("User {UserId} ({Username}) logged out from {IpAddress}. Session {SessionId} invalidated: {Result}",
                    userId, username, ipAddress, sessionId, result);

                return Ok(new
                {
                    message = "Logout successful",
                    sessionId = sessionId,
                    timestamp = DateTime.UtcNow,
                    isSuccess = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new
                {
                    message = "An error occurred during logout.",
                    isSuccess = false
                });
            }
        }

        // ✅ Helper method to extract client IP
        private string GetClientIpAddress()
        {
            string ipAddress = string.Empty;

            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim() ?? string.Empty;
            }
            else if (Request.Headers.ContainsKey("X-Real-IP"))
            {
                ipAddress = Request.Headers["X-Real-IP"].FirstOrDefault() ?? string.Empty;
            }
            else if (Request.Headers.ContainsKey("CF-Connecting-IP"))
            {
                ipAddress = Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? string.Empty;
            }

            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            }

            if (ipAddress == "::1")
            {
                ipAddress = "127.0.0.1";
            }

            return ipAddress;
        }
    }
}