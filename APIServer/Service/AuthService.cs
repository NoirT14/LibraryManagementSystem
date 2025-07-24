using APIServer.Data;
using APIServer.DTO.Auth;
using APIServer.Models;
using APIServer.Repositories.Interfaces;
using APIServer.Service.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace APIServer.Service
{
    public class SessionData
    {
        public string SessionId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
        public DateTime LastActivity { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public BrowserInfoDTO? BrowserInfo { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // ✅ NEW: OTP Data structure
    public class OtpData
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public DateTime ExpiryTime { get; set; }
        public int UserId { get; set; }
        public int AttemptCount { get; set; } = 0;
    }

    public class AuthService : IAuthService
    {
        private readonly LibraryDatabaseContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;

        // ✅ UPDATED: Replace old session tracking with new SessionData
        private static readonly Dictionary<string, SessionData> _activeSessions = new();

        // ✅ NEW: OTP storage (In production, consider using Redis or database)
        private static readonly Dictionary<string, OtpData> _otpStorage = new();
        private static readonly object _lock = new();

        public AuthService(LibraryDatabaseContext context, IConfiguration configuration, IEmailService emailService, IUserRepository userRepository, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<AuthResult> Authenticate(LoginRequestDTO loginRequest, string ipAddress, string userAgent)
        {
            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == loginRequest.UsernameorEmail || u.Email == loginRequest.UsernameorEmail);
            if (user == null)
            {
                return new AuthResult { IsSuccess = false, ErrorMessage = "Invalid username or email or password." };
            }

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginRequest.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return new AuthResult { IsSuccess = false, ErrorMessage = "Invalid username or email or password." };
            }

            var sessionId = Guid.NewGuid().ToString();
            var loginTime = DateTime.UtcNow;

            // ✅ UPDATED: Store session data for logout functionality
            lock (_lock)
            {
                _activeSessions[sessionId] = new SessionData
                {
                    SessionId = sessionId,
                    UserId = user.UserId,
                    Username = user.Username,
                    LoginTime = loginTime,
                    LastActivity = loginTime,
                    IpAddress = ipAddress,
                    BrowserInfo = loginRequest.BrowserInfo,
                    IsActive = true
                };
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim("name", user.FullName),
                new Claim("email", user.Email),
                new Claim("role", user.Role.RoleName),
                new Claim("phone", user.Phone ?? ""),
                new Claim("address", user.Address ?? ""),

                new Claim("sessionId", sessionId),
                new Claim("loginTime", loginTime.ToString("O")),
                new Claim("ipAddress", ipAddress),
                new Claim("userAgent", userAgent)
            };

            // ✅ Add browser info claims if available
            if (loginRequest.BrowserInfo != null)
            {
                if (!string.IsNullOrEmpty(loginRequest.BrowserInfo.BrowserName))
                    claims.Add(new Claim("browserName", loginRequest.BrowserInfo.BrowserName));
                if (!string.IsNullOrEmpty(loginRequest.BrowserInfo.BrowserVersion))
                    claims.Add(new Claim("browserVersion", loginRequest.BrowserInfo.BrowserVersion));
                if (!string.IsNullOrEmpty(loginRequest.BrowserInfo.OperatingSystem))
                    claims.Add(new Claim("os", loginRequest.BrowserInfo.OperatingSystem));
                if (!string.IsNullOrEmpty(loginRequest.BrowserInfo.Language))
                    claims.Add(new Claim("language", loginRequest.BrowserInfo.Language));
                if (!string.IsNullOrEmpty(loginRequest.BrowserInfo.Timezone))
                    claims.Add(new Claim("timezone", loginRequest.BrowserInfo.Timezone));
                if (!string.IsNullOrEmpty(loginRequest.BrowserInfo.ScreenResolution))
                    claims.Add(new Claim("screenResolution", loginRequest.BrowserInfo.ScreenResolution));

                // ✅ Full browser info as JSON
                var browserInfoJson = JsonSerializer.Serialize(loginRequest.BrowserInfo);
                claims.Add(new Claim("browserInfo", browserInfoJson));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = loginRequest.RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("User {UserId} ({Username}) logged in from {IpAddress} using {Browser} on {OS}",
                user.UserId, user.Username, ipAddress,
                loginRequest.BrowserInfo?.BrowserName ?? "Unknown",
                loginRequest.BrowserInfo?.OperatingSystem ?? "Unknown");

            // ✅ Prepare session info for response
            var sessionInfo = new SessionInfoDTO
            {
                SessionId = sessionId,
                LoginTime = loginTime,
                IpAddress = ipAddress,
                BrowserInfo = loginRequest.BrowserInfo
            };

            return new AuthResult
            {
                IsSuccess = true,
                Data = new LoginResponseDTO
                {
                    Token = tokenString,
                    Role = user.RoleId,
                    SessionInfo = sessionInfo,
                }
            };
        }

        public async Task<AuthResult> Register(RegisterRequestDTO registerRequest, string ipAddress, string userAgent)
        {
            if (await _context.Users.AnyAsync(u => u.Username == registerRequest.Username || u.Email == registerRequest.Email))
            {
                return new AuthResult { IsSuccess = false, ErrorMessage = "Username or Email already exists." };
            }

            var passwordHasher = new PasswordHasher<User>();

            var user = new User
            {
                Username = registerRequest.Username,
                FullName = registerRequest.FullName,
                Email = registerRequest.Email,
                Phone = registerRequest.Phone,
                Address = registerRequest.Address,
                RoleId = 3,
                CreateDate = DateTime.Now
            };

            user.PasswordHash = passwordHasher.HashPassword(user, registerRequest.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} ({Email}) registered from {IpAddress} using {Browser} on {OS}",
                registerRequest.Username, registerRequest.Email, ipAddress,
                registerRequest.BrowserInfo?.BrowserName ?? "Unknown",
                registerRequest.BrowserInfo?.OperatingSystem ?? "Unknown");

            return new AuthResult { IsSuccess = true };
        }

        // ✅ NEW: Invalidate session for logout
        public async Task<bool> InvalidateSessionAsync(string sessionId)
        {
            await Task.CompletedTask;

            lock (_lock)
            {
                if (_activeSessions.ContainsKey(sessionId))
                {
                    _activeSessions.Remove(sessionId);
                    _logger.LogInformation("Session {SessionId} invalidated successfully", sessionId);
                    return true;
                }

                _logger.LogWarning("Session {SessionId} not found for invalidation", sessionId);
                return false;
            }
        }

        // ✅ NEW: Cleanup expired sessions
        public async Task CleanupExpiredSessionsAsync()
        {
            await Task.CompletedTask;

            lock (_lock)
            {
                var cutoffTime = DateTime.UtcNow.AddHours(-24); // Remove sessions older than 24 hours
                var expiredSessions = _activeSessions
                    .Where(s => s.Value.LastActivity < cutoffTime)
                    .Select(s => s.Key)
                    .ToList();

                foreach (var sessionId in expiredSessions)
                {
                    _activeSessions.Remove(sessionId);
                }

                if (expiredSessions.Count > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count);
                }
            }
        }

        // ✅ UPDATED: Get analytics with accurate session data
        public async Task<Dictionary<string, object>> GetAnalyticsAsync()
        {
            await Task.CompletedTask;

            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var activeSessions = _activeSessions.Values
                    .Where(s => s.LastActivity > now.AddMinutes(-30))
                    .ToList();

                var browserStats = activeSessions
                    .Where(s => s.BrowserInfo?.BrowserName != null)
                    .GroupBy(s => s.BrowserInfo!.BrowserName)
                    .ToDictionary(g => g.Key, g => g.Count());

                var osStats = activeSessions
                    .Where(s => s.BrowserInfo?.OperatingSystem != null)
                    .GroupBy(s => s.BrowserInfo!.OperatingSystem)
                    .ToDictionary(g => g.Key, g => g.Count());

                return new Dictionary<string, object>
                {
                    ["BrowserDistribution"] = browserStats,
                    ["OperatingSystemDistribution"] = osStats,
                    ["ActiveSessionsLast30Minutes"] = activeSessions.Count,
                    ["TotalActiveSessions"] = _activeSessions.Count,
                    ["TotalSessions"] = _activeSessions.Count,
                    ["GeneratedAt"] = DateTime.UtcNow
                };
            }
        }

        // ✅ UPDATED: Send OTP instead of reset link
        public async Task<bool> SendResetPasswordOtpAsync(ForgotPasswordRequestDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.UsernameorEmail || u.Email == request.UsernameorEmail);
            if (user == null)
            {
                // Return true to prevent email enumeration attacks
                return true;
            }

            // Generate 6-digit OTP
            var random = new Random();
            var otpCode = random.Next(100000, 999999).ToString();
            var expiryTime = DateTime.UtcNow.AddMinutes(10); // OTP valid for 10 minutes

            // Store OTP
            lock (_lock)
            {
                _otpStorage[user.Email] = new OtpData
                {
                    Email = user.Email,
                    OtpCode = otpCode,
                    ExpiryTime = expiryTime,
                    UserId = user.UserId,
                    AttemptCount = 0
                };
            }

            string subject = "Password Reset OTP";
            string body = $"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>" +
                          $"<h2 style='color: #333;'>Password Reset Request</h2>" +
                          $"<p>Hi {user.FullName},</p>" +
                          $"<p>You requested to reset your password. Use the OTP code below to proceed:</p>" +
                          $"<div style='background-color: #f8f9fa; border: 2px solid #007bff; padding: 20px; text-align: center; margin: 20px 0; border-radius: 8px;'>" +
                          $"<h1 style='color: #007bff; font-size: 32px; margin: 0; letter-spacing: 5px;'>{otpCode}</h1>" +
                          $"</div>" +
                          $"<p><strong>Important:</strong></p>" +
                          $"<ul>" +
                          $"<li>This OTP is valid for <strong>10 minutes</strong> only</li>" +
                          $"<li>Do not share this code with anyone</li>" +
                          $"<li>If you didn't request this, please ignore this email</li>" +
                          $"</ul>" +
                          $"<p>Best regards,<br>DevTeam</p>" +
                          $"</div>";

            try
            {
                await _emailService.SendMailAsync(user.Email, subject, body);
                _logger.LogInformation("OTP sent to user {UserId} ({Email})", user.UserId, user.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP to {Email}", user.Email);
                return false;
            }
        }

        // ✅ FIXED: Return proper AuthResult with object instead of specific type
        public async Task<AuthResult> VerifyOtpAsync(VerifyOtpRequestDTO request)
        {
            await Task.CompletedTask;

            lock (_lock)
            {
                if (!_otpStorage.TryGetValue(request.Email, out var otpData))
                {
                    return new AuthResult { IsSuccess = false, ErrorMessage = "Invalid or expired OTP." };
                }

                // Check if OTP is expired
                if (DateTime.UtcNow > otpData.ExpiryTime)
                {
                    _otpStorage.Remove(request.Email);
                    return new AuthResult { IsSuccess = false, ErrorMessage = "OTP has expired. Please request a new one." };
                }

                // Check attempt limit (max 5 attempts)
                if (otpData.AttemptCount >= 5)
                {
                    _otpStorage.Remove(request.Email);
                    return new AuthResult { IsSuccess = false, ErrorMessage = "Too many failed attempts. Please request a new OTP." };
                }

                // Verify OTP
                if (otpData.OtpCode != request.OtpCode)
                {
                    otpData.AttemptCount++;
                    return new AuthResult { IsSuccess = false, ErrorMessage = $"Invalid OTP. {5 - otpData.AttemptCount} attempts remaining." };
                }

                // ✅ FIXED: Return object instead of specific DTO type
                return new AuthResult
                {
                    IsSuccess = true,
                    Data = new VerifyOtpResponseDTO
                    {
                        Email = request.Email,
                        UserId = otpData.UserId,
                        Message = "OTP verified successfully. You can now reset your password."
                    }
                };
            }
        }

        // ✅ FIXED: Return proper AuthResult with string message
        public async Task<AuthResult> ResetPasswordWithOtpAsync(ResetPasswordWithOtpRequestDTO request)
        {
            lock (_lock)
            {
                if (!_otpStorage.TryGetValue(request.Email, out var otpData))
                {
                    return new AuthResult { IsSuccess = false, ErrorMessage = "Invalid or expired session. Please start over." };
                }

                // Verify OTP again for security
                if (otpData.OtpCode != request.OtpCode || DateTime.UtcNow > otpData.ExpiryTime)
                {
                    _otpStorage.Remove(request.Email);
                    return new AuthResult { IsSuccess = false, ErrorMessage = "Invalid or expired OTP." };
                }
            }

            // Get user from database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return new AuthResult { IsSuccess = false, ErrorMessage = "User not found." };
            }

            // Update password
            var passwordHasher = new PasswordHasher<User>();
            user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);

            try
            {
                await _context.SaveChangesAsync();

                // Remove OTP from storage after successful password reset
                lock (_lock)
                {
                    _otpStorage.Remove(request.Email);
                }

                _logger.LogInformation("Password reset successfully for user {UserId} ({Email})", user.UserId, user.Email);

                // ✅ FIXED: Return string message as object, not direct string
                return new AuthResult
                {
                    IsSuccess = true,
                    Data = new { Message = "Password has been reset successfully." }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset password for user {UserId}", user.UserId);
                return new AuthResult { IsSuccess = false, ErrorMessage = "Failed to reset password. Please try again." };
            }
        }

        // ✅ NEW: Cleanup expired OTPs
        public async Task CleanupExpiredOtpsAsync()
        {
            await Task.CompletedTask;

            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var expiredOtps = _otpStorage
                    .Where(o => o.Value.ExpiryTime < now)
                    .Select(o => o.Key)
                    .ToList();

                foreach (var email in expiredOtps)
                {
                    _otpStorage.Remove(email);
                }

                if (expiredOtps.Count > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} expired OTPs", expiredOtps.Count);
                }
            }
        }

        // ✅ LEGACY: Keep old method for backward compatibility (now calls OTP method)
        public async Task<bool> SendResetPasswordTokenAsync(ForgotPasswordRequestDTO request)
        {
            return await SendResetPasswordOtpAsync(request);
        }

        // ✅ LEGACY: Keep old method for backward compatibility
        public async Task<AuthResult> ResetPassword(ResetPasswordRequestDTO resetRequest)
        {
            // This method is deprecated, redirect users to use OTP method
            return new AuthResult
            {
                IsSuccess = false,
                ErrorMessage = "This method is no longer supported. Please use the OTP-based password reset."
            };
        }

        public async Task<bool> IsEmailTaken(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user != null;
        }

        public async Task<bool> IsUsernameTaken(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user != null;
        }

        public async Task<bool> ValidateSessionFingerprintAsync(string sessionId, BrowserInfoDTO currentBrowserInfo)
        {
            await Task.CompletedTask;

            lock (_lock)
            {
                if (!_activeSessions.TryGetValue(sessionId, out var session))
                {
                    _logger.LogWarning("Session {SessionId} not found for fingerprint validation", sessionId);
                    return false;
                }

                if (session.BrowserInfo == null || currentBrowserInfo == null)
                {
                    _logger.LogInformation("Skipping fingerprint validation - missing browser info for session {SessionId}", sessionId);
                    return true; // Skip validation if no browser info
                }

                // Define critical fields that shouldn't change
                var criticalFieldsMatch = CompareCriticalFingerprint(session.BrowserInfo, currentBrowserInfo);

                if (!criticalFieldsMatch)
                {
                    _logger.LogWarning("Session {SessionId} critical fingerprint mismatch. User: {UserId}, Original: {OriginalInfo}, Current: {CurrentInfo}",
                        sessionId, session.UserId,
                        SerializeBrowserInfo(session.BrowserInfo),
                        SerializeBrowserInfo(currentBrowserInfo));

                    // Invalidate session
                    _activeSessions.Remove(sessionId);
                    return false;
                }

                // Update last activity time
                session.LastActivity = DateTime.UtcNow;

                _logger.LogDebug("Session {SessionId} fingerprint validation passed", sessionId);
                return true;
            }
        }

        private bool CompareCriticalFingerprint(BrowserInfoDTO stored, BrowserInfoDTO current)
        {
            // Critical fields that rarely change - strict validation
            var criticalMatch =
                stored.ScreenResolution == current.ScreenResolution &&
                stored.Timezone == current.Timezone &&
                stored.Language == current.Language &&
                stored.OperatingSystem == current.OperatingSystem;

            // Optional: Also check browser but allow version updates
            var browserMatch = stored.BrowserName == current.BrowserName;

            return criticalMatch && browserMatch;
        }

        private string SerializeBrowserInfo(BrowserInfoDTO browserInfo)
        {
            try
            {
                return JsonSerializer.Serialize(browserInfo);
            }
            catch
            {
                return "SerializationError";
            }
        }
    }
}