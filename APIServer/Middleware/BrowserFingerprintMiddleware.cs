using APIServer.DTO.Auth;
using APIServer.Service.Interfaces;
using System.Text.Json;

namespace APIServer.Middleware
{
    public class BrowserFingerprintMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BrowserFingerprintMiddleware> _logger;

        public BrowserFingerprintMiddleware(RequestDelegate next, IServiceProvider serviceProvider, ILogger<BrowserFingerprintMiddleware> logger)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip validation for non-authenticated requests
            if (!context.User.Identity?.IsAuthenticated == true)
            {
                await _next(context);
                return;
            }

            // Skip validation for auth endpoints to avoid infinite loops
            if (IsAuthEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            try
            {
                var sessionId = context.User.FindFirst("sessionId")?.Value;

                if (string.IsNullOrEmpty(sessionId))
                {
                    await ReturnUnauthorized(context, "Invalid session");
                    return;
                }

                // ✅ Extract STORED browser info từ JWT token (browser info khi login)
                var storedBrowserInfo = ExtractBrowserInfoFromToken(context.User);

                // ✅ Extract CURRENT browser info từ request hiện tại
                var currentBrowserInfo = ExtractBrowserInfoFromHeaders(context.Request);

                // ✅ Log để debug
                _logger.LogInformation("Browser fingerprint check - Session: {SessionId}, Stored: {StoredBrowser}, Current: {CurrentBrowser}",
                    sessionId, storedBrowserInfo.BrowserName, currentBrowserInfo.BrowserName);

                // ✅ Compare browser fingerprint trực tiếp trong middleware
                var isValid = CompareBrowserFingerprint(storedBrowserInfo, currentBrowserInfo);

                if (!isValid)
                {
                    _logger.LogWarning("Browser fingerprint mismatch for session {SessionId}. Stored: {StoredInfo}, Current: {CurrentInfo}",
                        sessionId,
                        JsonSerializer.Serialize(storedBrowserInfo),
                        JsonSerializer.Serialize(currentBrowserInfo));

                    // ✅ Invalidate session khi detect browser khác
                    using var scope = _serviceProvider.CreateScope();
                    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                    await authService.InvalidateSessionAsync(sessionId);

                    await ReturnUnauthorized(context, "Browser fingerprint mismatch. Please login again.");
                    return;
                }

                _logger.LogDebug("Browser fingerprint validation passed for session {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during browser fingerprint validation");
                // Continue request on validation error để tránh break app
            }

            await _next(context);
        }

        private bool IsAuthEndpoint(PathString path)
        {
            var authPaths = new[] {
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/logout",
                "/api/auth/forgot-password",
                "/api/auth/reset-password"
            };
            return authPaths.Any(authPath => path.StartsWithSegments(authPath, StringComparison.OrdinalIgnoreCase));
        }

        private BrowserInfoDTO ExtractBrowserInfoFromToken(System.Security.Claims.ClaimsPrincipal user)
        {
            // Extract STORED browser info từ JWT token claims (browser info lúc login)
            return new BrowserInfoDTO
            {
                BrowserName = user.FindFirst("browserName")?.Value ?? "Unknown",
                BrowserVersion = user.FindFirst("browserVersion")?.Value ?? "Unknown",
                OperatingSystem = user.FindFirst("os")?.Value ?? "Unknown",
                Language = user.FindFirst("language")?.Value ?? "Unknown",
                Timezone = user.FindFirst("timezone")?.Value ?? "Unknown",
                ScreenResolution = user.FindFirst("screenResolution")?.Value ?? "Unknown",
                UserAgent = user.FindFirst("userAgent")?.Value ?? "Unknown"
            };
        }

        private BrowserInfoDTO ExtractBrowserInfoFromHeaders(HttpRequest request)
        {
            // Extract CURRENT browser info từ request headers
            var userAgent = request.Headers["User-Agent"].FirstOrDefault() ?? "";

            return new BrowserInfoDTO
            {
                // ✅ Ưu tiên custom headers, fallback to User-Agent parsing
                BrowserName = request.Headers["X-Browser-Name"].FirstOrDefault() ?? ParseBrowserFromUserAgent(userAgent),
                BrowserVersion = request.Headers["X-Browser-Version"].FirstOrDefault() ?? ParseBrowserVersionFromUserAgent(userAgent),
                OperatingSystem = request.Headers["X-Operating-System"].FirstOrDefault() ?? ParseOSFromUserAgent(userAgent),
                Language = request.Headers["X-Language"].FirstOrDefault() ?? request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0] ?? "Unknown",
                Timezone = request.Headers["X-Timezone"].FirstOrDefault() ?? "Unknown",
                ScreenResolution = request.Headers["X-Screen-Resolution"].FirstOrDefault() ?? "Unknown",
                UserAgent = userAgent
            };
        }

        private string ParseBrowserFromUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            userAgent = userAgent.ToLower();

            // ✅ Thứ tự quan trọng: Edge phải check trước Chrome
            if (userAgent.Contains("edg/")) return "Edge";
            if (userAgent.Contains("opr/") || userAgent.Contains("opera")) return "Opera";
            if (userAgent.Contains("firefox")) return "Firefox";
            if (userAgent.Contains("chrome")) return "Chrome";
            if (userAgent.Contains("safari") && !userAgent.Contains("chrome")) return "Safari";

            return "Unknown";
        }

        private string ParseBrowserVersionFromUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            // Simple version extraction
            var patterns = new Dictionary<string, string>
            {
                { "Chrome", @"Chrome/(\d+)" },
                { "Firefox", @"Firefox/(\d+)" },
                { "Edge", @"Edg/(\d+)" },
                { "Opera", @"OPR/(\d+)" },
                { "Safari", @"Version/(\d+)" }
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(userAgent, pattern.Value);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return "Unknown";
        }

        private string ParseOSFromUserAgent(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return "Unknown";

            userAgent = userAgent.ToLower();

            if (userAgent.Contains("windows nt 10")) return "Windows 10";
            if (userAgent.Contains("windows nt 6.3")) return "Windows 8.1";
            if (userAgent.Contains("windows nt 6.2")) return "Windows 8";
            if (userAgent.Contains("windows nt 6.1")) return "Windows 7";
            if (userAgent.Contains("windows")) return "Windows";
            if (userAgent.Contains("macintosh") || userAgent.Contains("mac os x")) return "macOS";
            if (userAgent.Contains("linux")) return "Linux";
            if (userAgent.Contains("android")) return "Android";
            if (userAgent.Contains("iphone") || userAgent.Contains("ipad")) return "iOS";

            return "Unknown";
        }

        private bool CompareBrowserFingerprint(BrowserInfoDTO stored, BrowserInfoDTO current)
        {
            // ✅ Critical fields check theo yêu cầu của bạn
            var browserMatch = stored.BrowserName?.Equals(current.BrowserName, StringComparison.OrdinalIgnoreCase) == true;

            // ✅ Optional: Thêm các check khác nếu muốn strict hơn
            var screenMatch = stored.ScreenResolution?.Equals(current.ScreenResolution, StringComparison.OrdinalIgnoreCase) == true;
            var osMatch = stored.OperatingSystem?.Equals(current.OperatingSystem, StringComparison.OrdinalIgnoreCase) == true;
            var timezoneMatch = stored.Timezone?.Equals(current.Timezone, StringComparison.OrdinalIgnoreCase) == true;

            // ✅ Theo yêu cầu: Chủ yếu check browser name
            // Có thể customize logic này theo nhu cầu:
            // - Chỉ check browser: return browserMatch;
            // - Check browser + OS: return browserMatch && osMatch;
            // - Check tất cả: return browserMatch && screenMatch && osMatch && timezoneMatch;

            return browserMatch; // ✅ Chỉ check browser name theo yêu cầu
        }

        private async Task ReturnUnauthorized(HttpContext context, string message)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new
            {
                message = message,
                requireRelogin = true,
                timestamp = DateTime.UtcNow
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}