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

                    // ✅ Simple cleanup every 2 hours
                    await authService.CleanupExpiredSessionsAsync();

                    await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in session cleanup service");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // Retry after 10 min
                }
            }
        }
    }
}
