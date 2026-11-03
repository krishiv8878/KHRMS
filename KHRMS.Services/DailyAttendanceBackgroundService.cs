using System;
using System.Threading;
using System.Threading.Tasks;
using KHRMS.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace KHRMS.Services
{
    public class DailyAttendanceBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

        public DailyAttendanceBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("DailyAttendanceBackgroundService started. Periodic daily attendance status & auto clock-out worker active.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAttendanceComputationAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error occurred during daily attendance computation cycle.");
                }

                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            Log.Information("DailyAttendanceBackgroundService stopped.");
        }

        private async Task ProcessAttendanceComputationAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dailyAttendanceService = scope.ServiceProvider.GetRequiredService<IDailyAttendanceService>();

            // 1. Process auto clock-out for open punches past shift buffer
            try
            {
                int autoClockedOut = await dailyAttendanceService.RunAutoClockOutAsync();
                if (autoClockedOut > 0)
                {
                    Log.Information("DailyAttendanceBackgroundService auto-clocked out {Count} open punches.", autoClockedOut);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to run auto clock-out process.");
            }

            // 2. Compute attendance records for yesterday and today
            var today = DateOnly.FromDateTime(DateTime.Today);
            var yesterday = today.AddDays(-1);

            try
            {
                int yesterdayComputed = await dailyAttendanceService.ComputeDailyAttendanceAsync(yesterday);
                int todayComputed = await dailyAttendanceService.ComputeDailyAttendanceAsync(today);

                Log.Information("DailyAttendanceBackgroundService computed records: Yesterday={YesterdayCount}, Today={TodayCount}",
                    yesterdayComputed, todayComputed);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to compute daily attendance records.");
            }
        }
    }
}
