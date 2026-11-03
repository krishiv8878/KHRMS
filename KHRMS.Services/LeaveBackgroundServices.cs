using KHRMS.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace KHRMS.Services
{
    /// <summary>
    /// Background service that runs on the 1st of every month to process leave accruals idempotently.
    /// </summary>
    public class LeaveAccrualBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

        public LeaveAccrualBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("LeaveAccrualBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    // Run accrual check
                    using var scope = _serviceProvider.CreateScope();
                    var ledgerService = scope.ServiceProvider.GetRequiredService<ILeaveLedgerService>();

                    await ledgerService.ProcessMonthlyAccrualAsync(now.Year, now.Month);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in LeaveAccrualBackgroundService execution.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }

    /// <summary>
    /// Background service that runs at leave-year boundaries to carry forward or lapse/encash balances.
    /// </summary>
    public class LeaveRolloverBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

        public LeaveRolloverBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("LeaveRolloverBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    // Check rollover in January (or early in year)
                    if (now.Month == 1)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var ledgerService = scope.ServiceProvider.GetRequiredService<ILeaveLedgerService>();

                        await ledgerService.ProcessYearEndRolloverAsync(now.Year - 1, now.Year);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in LeaveRolloverBackgroundService execution.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }

    /// <summary>
    /// Background service that runs daily to expire past-due Comp-Off credits.
    /// </summary>
    public class CompOffExpiryBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(4);

        public CompOffExpiryBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("CompOffExpiryBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var ledgerService = scope.ServiceProvider.GetRequiredService<ILeaveLedgerService>();

                    await ledgerService.ProcessCompOffExpiryAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in CompOffExpiryBackgroundService execution.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
