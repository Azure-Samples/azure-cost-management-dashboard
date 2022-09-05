using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using services.APIs.CostManagement;
using services.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace dashboard
{
    public class MailWarningWorker : IHostedService, IDisposable
    {
        private readonly ILogger<MailWarningWorker> _logger;
        private Timer _timer = null;

        public MailWarningWorker(ILogger<MailWarningWorker> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("API Service: Serviço rodando.");

            _timer = new Timer(RunMailWarningWorker, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(Utils.RunEmailRescheduleInSeconds));

            return Task.CompletedTask;
        }

        private void RunMailWarningWorker(object state)
        {
            _logger.LogWarning("API Service: Executou chamada de API RunMailWarningWorker.");

            var task = Task.Run(() => CostManagementService.NotifyConsumptionIncreaseByEmailAsync());
            task.Wait();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("API Service MailWarningWorker: Serviço está sendo parado.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
