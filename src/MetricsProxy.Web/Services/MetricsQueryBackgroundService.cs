using System;
using System.Threading;
using System.Threading.Tasks;
using MetricsProxy.Application.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MetricsProxy.Web.Services
{
    public record QueryServiceOptions()
    {
        public int IntervalInMilliseconds { get; init; } = 10000;
    }
    
    public class MetricsQueryBackgroundService : BackgroundService
    {
        private readonly IOptions<QueryServiceOptions> _options;
        private readonly IMetricsManagementService _metricsManagementService;
        private readonly ILogger<MetricsQueryBackgroundService> _logger;
        private readonly IBackgroundServiceTracker _tracker;

        public MetricsQueryBackgroundService(
            IOptions<QueryServiceOptions> options, 
            IMetricsManagementService metricsManagementService, 
            ILogger<MetricsQueryBackgroundService> logger,
            IBackgroundServiceTracker tracker)
        {
            _options = options;
            _metricsManagementService = metricsManagementService;
            _logger = logger;
            _tracker = tracker;
            _tracker.Report("Not started");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _tracker.Report($"Running ({_options.Value.IntervalInMilliseconds} ms interval)");
                try
                {
                    await _metricsManagementService.QueryAndReport(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"An exception occurred in the background service. The current cycle may not have been completed. Will retry in {_options.Value.IntervalInMilliseconds/100} seconds!");
                }
                finally
                {
                    await Task.Delay(_options.Value.IntervalInMilliseconds, stoppingToken);
                }
            }

            _tracker.Report("Stopped");
        }
    }
}