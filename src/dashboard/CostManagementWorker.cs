using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using services.APIs.CostManagement;
using services.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace dashboard
{
    public class CostManagementWorker : IHostedService, IDisposable
    {
        private readonly ILogger<CostManagementWorker> _logger;
        private Timer _timer = null;

        public CostManagementWorker(ILogger<CostManagementWorker> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("API Service: Serviço rodando.");

            _timer = new Timer(RunCostManagementService, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(Utils.RunCostManagemementApiInSeconds));

            return Task.CompletedTask;
        }

        private void RunCostManagementService(object state)
        {
            _logger.LogWarning("API Service CostManagementWorker: Executou chamada de API GetBillingMonthToDate.");

            var task = Task.Run(async () => await CostManagementService.AzureBillingMonthToDateApiFetchAsync());
            task.Wait();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("API Service: Serviço está sendo parado.");

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
