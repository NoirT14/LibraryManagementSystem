using APIServer.Service.Interfaces;

namespace APIServer.Service
{
    public class SessionCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SessionCleanupService> _logger;

        public SessionCleanupService(IServiceProvider serviceProvider, ILogger<SessionCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Session cleanup service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                    // ✅ Cleanup expired sessions
                    await authService.CleanupExpiredSessionsAsync();

                    // ✅ NEW: Cleanup expired OTPs
                    await authService.CleanupExpiredOtpsAsync();

                    _logger.LogDebug("Cleanup completed successfully at {Time}", DateTime.UtcNow);

                    // ✅ Run cleanup every 2 hours
                    await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in session cleanup service");
                    // ✅ Retry after 10 minutes on error
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }

            _logger.LogInformation("Session cleanup service stopped");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Session cleanup service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}