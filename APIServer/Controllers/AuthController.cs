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

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            var result = await _authService.ResetPassword(request);
            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { message = "Password reset successfully!" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {
            var result = await _authService.SendResetPasswordTokenAsync(request);

            return Ok(new { message = "If the email exists, a reset password link has been sent." });
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
    }
}
