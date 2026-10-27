using System;
using System.Threading;
using System.Threading.Tasks;
using KHRMS.Services.Workflow.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace KHRMS.Services.Workflow.Services
{
    public class WorkflowEscalationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(2); // Runs every 2 minutes

        public WorkflowEscalationBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("WorkflowEscalationBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var workflowEngine = scope.ServiceProvider.GetRequiredService<IWorkflowEngineService>();
                    await workflowEngine.EscalateOverdueStepsAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error occurred during workflow step escalation execution.");
                }

                try
                {
                    await Task.Delay(_period, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            Log.Information("WorkflowEscalationBackgroundService stopped.");
        }
    }
}
