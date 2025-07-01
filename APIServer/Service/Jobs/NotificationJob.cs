using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using APIServer.Service.Interfaces;

namespace APIServer.Service.Jobs
{
    public class NotificationJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public NotificationJob(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var loanService = scope.ServiceProvider.GetRequiredService<ILoanService>();
                var ReservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();

                try
                {
                    await loanService.UpdateOverdueLoansAndFinesAsync();
                    await loanService.SendDueDateRemindersAsync();
                    await loanService.SendFineNotificationsAsync();
                    await ReservationService.CheckAvailableReservationsAsync();
                    await ReservationService.ExpireOldReservationsAsync();
                }
                catch (Exception ex)
                {
                    // Log lỗi nếu cần
                    Console.WriteLine($"Error sending reminders: {ex.Message}");
                }

                // Đợi 24h rồi chạy lại (hoặc đổi thành TimeSpan ngắn hơn nếu test)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}

