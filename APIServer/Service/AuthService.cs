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

    public class AuthService : IAuthService
    {
        private readonly LibraryDatabaseContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;

        // ✅ UPDATED: Replace old session tracking with new SessionData
        private static readonly Dictionary<string, SessionData> _activeSessions = new();
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

        public async Task<AuthResult> ResetPassword(ResetPasswordRequestDTO resetRequest)
        {
            if (string.IsNullOrEmpty(resetRequest.Token))
            {
                return new AuthResult { IsSuccess = false, ErrorMessage = "Token is required." };
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken;

            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured"));

            try
            {
                var princical = tokenHandler.ValidateToken(resetRequest.Token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out validatedToken);

                var purposeClaim = princical.FindFirst("purpose")?.Value;
                if (purposeClaim != "reset-password")
                {
                    return new AuthResult { IsSuccess = false, ErrorMessage = "Invalid token purpose." };
                }

                var userIdClaim = princical.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var emailClaim = princical.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return new AuthResult { IsSuccess = false, ErrorMessage = "Invalid token user." };
                }
                if (string.IsNullOrEmpty(emailClaim))
                {
                    return new AuthResult { IsSuccess = false, ErrorMessage = "Invalid token email." };
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.Email == emailClaim);
                if (user == null)
                {
                    return new AuthResult { IsSuccess = false, ErrorMessage = "User not found." };
                }

                var passwordHasher = new PasswordHasher<User>();
                user.PasswordHash = passwordHasher.HashPassword(user, resetRequest.NewPassword);

                await _context.SaveChangesAsync();

                return new AuthResult { IsSuccess = true };
            }
            catch (SecurityTokenExpiredException)
            {
                return new AuthResult { IsSuccess = false, ErrorMessage = "Token has expired." };
            }
            catch (Exception)
            {
                return new AuthResult { IsSuccess = false, ErrorMessage = "Invalid token." };
            }
        }

        public async Task<bool> SendResetPasswordTokenAsync(ForgotPasswordRequestDTO request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.UsernameorEmail || u.Email == request.UsernameorEmail);
            if (user == null)
            {
                return false;
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("purpose", "reset-password")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddMinutes(10);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            string resetLink = $"{_configuration["Frontend:ResetPasswordUrl"]}?token={tokenString}";

            string subject = "Password Reset Request";
            string body = $"<p>Hi {user.FullName},</p>" +
                          $"<p>You requested to reset your password. Click the link below to reset it. The link is valid for 10 minutes.</p>" +
                          $"<p><a href='{resetLink}'>Reset Password</a></p>" +
                          $"<p>If you didn't request this, please ignore this email.</p>";

            await _emailService.SendMailAsync(user.Email, subject, body);

            return true;
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
    }
}