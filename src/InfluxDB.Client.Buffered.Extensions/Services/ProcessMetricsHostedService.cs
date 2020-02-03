using System;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Buffered.Extensions.Metrics;
using Microsoft.Extensions.Hosting;

namespace InfluxDB.Client.Buffered.Extensions.Services
{
    public class ProcessMetricsHostedService : IHostedService, IDisposable
    {
        private readonly IDefaultMetrics _metrics;
        private readonly ProcessMetricsOptions _options;
        private Timer _timer;

        public ProcessMetricsHostedService(IDefaultMetrics metrics, ProcessMetricsOptions options)
        {
            _metrics = metrics;
            _options = options;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            var interval = _options.PublishInterval;
            if (interval > TimeSpan.Zero)
            {
                _timer = new Timer(DoWork, null, TimeSpan.Zero, interval);
            }

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _metrics.PushProcessMetrics();
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }

}
